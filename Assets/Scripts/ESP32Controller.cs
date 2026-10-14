
using UnityEngine;
using System;
using System.Net;
using System.Net.Sockets;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;

public class ESP32Controller : MonoBehaviour
{
    // =====================================================
    // CONFIGURAÇÃO
    // =====================================================

    private const int portaTCP = 8080;
    private const int tempoTesteIP = 500;
    private const int tempoConexao = 3000;
    private const int intervaloReconexao = 2000;

    // =====================================================
    // CONEXÃO
    // =====================================================

    private TcpClient cliente;
    private NetworkStream stream;

    private bool conectado = false;
    private bool conectando = false;
    private bool ativo = true;

    private string enderecoESP32;

    // Evita que dois comandos escrevam no TCP ao mesmo tempo.
    private readonly SemaphoreSlim travaEnvio =
        new SemaphoreSlim(1, 1);

    // =====================================================
    // INICIALIZAÇÃO
    // =====================================================

    private async void Start()
    {
        Debug.Log("=================================");
        Debug.Log("ESP32 CONTROLLER INICIADO");
        Debug.Log("=================================");

        await ConectarContinuamente();
    }

    // =====================================================
    // CONEXÃO CONTÍNUA
    // =====================================================

    private async Task ConectarContinuamente()
    {
        while (ativo)
        {
            if (!conectado && !conectando)
            {
                await DescobrirEConectar();
            }

            if (!ativo)
                break;

            await Task.Delay(intervaloReconexao);
        }
    }

    // =====================================================
    // DESCOBRIR ESP32
    // =====================================================

    private async Task<string> DescobrirESP32()
    {
        Debug.Log("Procurando ESP32 na rede local...");

        List<string> enderecos = ObterEnderecosDaRede();

        if (enderecos.Count == 0)
        {
            Debug.LogWarning(
                "Não foi possível descobrir a rede local."
            );

            return null;
        }

        Debug.Log(
            "Endereços para testar: " + enderecos.Count
        );

        List<Task<string>> tarefas = new List<Task<string>>();

        foreach (string ip in enderecos)
        {
            if (!ativo)
                break;

            tarefas.Add(TestarIP(ip));
        }

        while (tarefas.Count > 0 && ativo)
        {
            Task<string> tarefa = await Task.WhenAny(tarefas);
            tarefas.Remove(tarefa);

            string resultado = null;

            try
            {
                resultado = await tarefa;
            }
            catch (Exception erro)
            {
                Debug.LogWarning(
                    "Erro ao testar endereço: " + erro.Message
                );
            }

            if (!string.IsNullOrEmpty(resultado))
            {
                Debug.Log("ESP32 encontrado em: " + resultado);
                return resultado;
            }
        }

        if (ativo)
        {
            Debug.LogWarning(
                "Nenhum ESP32 encontrado. Tentarei novamente."
            );
        }

        return null;
    }

    // =====================================================
    // OBTER ENDEREÇOS DA REDE LOCAL
    // =====================================================

    private List<string> ObterEnderecosDaRede()
    {
        HashSet<string> enderecos = new HashSet<string>();

        try
        {
            foreach (
                NetworkInterface rede
                in NetworkInterface.GetAllNetworkInterfaces()
            )
            {
                if (!ativo)
                    break;

                if (
                    rede.OperationalStatus !=
                    OperationalStatus.Up
                )
                    continue;

                if (
                    rede.NetworkInterfaceType ==
                    NetworkInterfaceType.Loopback
                )
                    continue;

                IPInterfaceProperties propriedades =
                    rede.GetIPProperties();

                foreach (
                    UnicastIPAddressInformation endereco
                    in propriedades.UnicastAddresses
                )
                {
                    if (
                        endereco.Address.AddressFamily !=
                        AddressFamily.InterNetwork
                    )
                        continue;

                    IPAddress ip = endereco.Address;
                    IPAddress mascara = endereco.IPv4Mask;

                    if (mascara == null)
                        continue;

                    byte[] ipBytes = ip.GetAddressBytes();
                    byte[] mascaraBytes = mascara.GetAddressBytes();

                    byte[] redeBytes = new byte[4];

                    for (int i = 0; i < 4; i++)
                    {
                        redeBytes[i] = (byte)(
                            ipBytes[i] & mascaraBytes[i]
                        );
                    }

                    uint inicioRede = EnderecoParaUInt(redeBytes);
                    uint mascaraNumero = EnderecoParaUInt(
                        mascaraBytes
                    );

                    uint broadcast =
                        inicioRede | (~mascaraNumero);

                    // Evita testar a própria rede e o broadcast.
                    if (broadcast <= inicioRede + 1)
                        continue;

                    uint inicio = inicioRede + 1;
                    uint fim = broadcast - 1;

                    // Evita varreduras enormes.
                    if (fim - inicio > 1024)
                    {
                        Debug.LogWarning(
                            "Rede muito grande; ignorando interface: "
                            + ip
                        );

                        continue;
                    }

                    Debug.Log(
                        "Rede detectada: " + ip + " / " + mascara
                    );

                    for (uint numero = inicio; numero <= fim; numero++)
                    {
                        string enderecoIP = UIntParaEndereco(numero);

                        // Não testa o próprio dispositivo.
                        if (enderecoIP == ip.ToString())
                            continue;

                        enderecos.Add(enderecoIP);
                    }
                }
            }
        }
        catch (Exception erro)
        {
            Debug.LogWarning(
                "Erro ao obter endereços da rede: " + erro.Message
            );
        }

        return new List<string>(enderecos);
    }

    // =====================================================
    // CONVERSÃO DE ENDEREÇO
    // =====================================================

    private uint EnderecoParaUInt(byte[] bytes)
    {
        return
            ((uint)bytes[0] << 24) |
            ((uint)bytes[1] << 16) |
            ((uint)bytes[2] << 8) |
            bytes[3];
    }

    private string UIntParaEndereco(uint valor)
    {
        byte a = (byte)((valor >> 24) & 255);
        byte b = (byte)((valor >> 16) & 255);
        byte c = (byte)((valor >> 8) & 255);
        byte d = (byte)(valor & 255);

        return $"{a}.{b}.{c}.{d}";
    }

    // =====================================================
    // TESTAR IP
    // =====================================================

    private async Task<string> TestarIP(string ip)
    {
        TcpClient teste = new TcpClient();

        try
        {
            Task conexao = teste.ConnectAsync(ip, portaTCP);

            Task terminou = await Task.WhenAny(
                conexao,
                Task.Delay(tempoTesteIP)
            );

            if (terminou != conexao)
                return null;

            // Observa possíveis erros da conexão.
            await conexao;

            if (teste.Connected)
                return ip;

            return null;
        }
        catch
        {
            return null;
        }
        finally
        {
            teste.Close();
        }
    }

    // =====================================================
    // DESCOBRIR E CONECTAR
    // =====================================================

    private async Task DescobrirEConectar()
    {
        if (conectando || !ativo)
            return;

        conectando = true;

        try
        {
            enderecoESP32 = await DescobrirESP32();

            if (
                !ativo ||
                string.IsNullOrEmpty(enderecoESP32)
            )
                return;

            await Conectar();
        }
        catch (Exception erro)
        {
            Debug.LogWarning(
                "Erro durante a conexão: " + erro.Message
            );
        }
        finally
        {
            conectando = false;
        }
    }

    // =====================================================
    // CONECTAR POR TCP
    // =====================================================

    private async Task Conectar()
    {
        if (
            !ativo ||
            string.IsNullOrEmpty(enderecoESP32)
        )
            return;

        Debug.Log(
            "Conectando ao ESP32 em " +
            enderecoESP32 + ":" + portaTCP
        );

        TcpClient novoCliente = new TcpClient();

        try
        {
            Task conexao = novoCliente.ConnectAsync(
                enderecoESP32,
                portaTCP
            );

            Task terminou = await Task.WhenAny(
                conexao,
                Task.Delay(tempoConexao)
            );

            if (terminou != conexao)
            {
                Debug.LogWarning(
                    "Tempo esgotado ao conectar ao ESP32."
                );

                return;
            }

            await conexao;

            if (!ativo || !novoCliente.Connected)
                return;

            cliente = novoCliente;
            stream = cliente.GetStream();
            conectado = true;

            // A propriedade do cliente passa para a classe.
            novoCliente = null;

            Debug.Log("=================================");
            Debug.Log("ESP32 CONECTADO COM SUCESSO!");
            Debug.Log("IP: " + enderecoESP32);
            Debug.Log("=================================");
        }
        catch (Exception erro)
        {
            Debug.LogWarning(
                "Erro ao conectar: " + erro.Message
            );

            Desconectar();
        }
        finally
        {
            if (novoCliente != null)
                novoCliente.Close();
        }
    }

    // =====================================================
    // PROGRAMAÇÃO DIÁRIA GLOBAL
    // =====================================================

    public void ProgramarLuzes(
        string horarioLigar,
        string horarioDesligar
    )
    {
        if (
            string.IsNullOrWhiteSpace(horarioLigar) ||
            string.IsNullOrWhiteSpace(horarioDesligar)
        )
        {
            Debug.LogWarning(
                "Informe os horários para programar as luzes."
            );

            return;
        }

        string comando =
            $"PROGRAMAR:{horarioLigar},{horarioDesligar}";

        Debug.Log(
            "Programando luzes: " +
            horarioLigar + " -> " + horarioDesligar
        );

        EnviarComando(comando);
    }

    public void CancelarProgramacao()
    {
        Debug.Log("Cancelando programação das luzes...");

        EnviarComando("CANCELAR_PROGRAMACAO");
    }

    // =====================================================
    // CONTROLE INDIVIDUAL POR CÔMODO
    // Cômodo 1 = Sala de Estar
    // Cômodo 2 = Cozinha
    // Cômodo 3 = Quarto
    // Cômodo 4 = Banheiro
    // =====================================================

    public void AlternarCor(bool ativo)
    {
        if (GerenciadorComodos.Instance == null)
            return;

        int numeroComodo =
            GerenciadorComodos.Instance.NumeroComodoAtual;

        if (ativo)
            LigarComodo(numeroComodo);
        else
            DesligarComodo(numeroComodo);
    }

    public void LigarComodo(int numeroComodo)
    {
        if (!NumeroComodoValido(numeroComodo))
            return;

        EnviarComando($"ROOM_ON:{numeroComodo}");
    }

    public void DesligarComodo(int numeroComodo)
    {
        if (!NumeroComodoValido(numeroComodo))
            return;

        EnviarComando($"ROOM_OFF:{numeroComodo}");
    }

    // Envia cor e intensidade para apenas uma lâmpada.
    public void EnviarCorComodo(
        int numeroComodo,
        Color cor,
        float intensidade
    )
    {
        if (!NumeroComodoValido(numeroComodo))
            return;

        intensidade = Mathf.Clamp01(intensidade);

        int r = Mathf.RoundToInt(
            Mathf.Clamp01(cor.r) * intensidade * 255f
        );

        int g = Mathf.RoundToInt(
            Mathf.Clamp01(cor.g) * intensidade * 255f
        );

        int b = Mathf.RoundToInt(
            Mathf.Clamp01(cor.b) * intensidade * 255f
        );

        EnviarComando(
            $"ROOM_RGB:{numeroComodo},{r},{g},{b}"
        );
    }

    private bool NumeroComodoValido(int numeroComodo)
    {
        if (numeroComodo >= 1 && numeroComodo <= 4)
            return true;

        Debug.LogWarning(
            "Número de cômodo inválido: " + numeroComodo
        );

        return false;
    }

    // Compatibilidade com chamadas antigas do ColorPicker.
    public void EnviarCor(Color cor)
    {
        if (GerenciadorComodos.Instance == null)
            return;

        EnviarCorComodo(
            GerenciadorComodos.Instance.NumeroComodoAtual,
            cor,
            GerenciadorComodos.Instance.IntensidadeAtual
        );
    }

    // Atualiza a intensidade mantendo a cor salva no cômodo.
    public void DefinirIntensidade(float intensidade)
    {
        if (GerenciadorComodos.Instance == null)
            return;

        GerenciadorComodos gerenciador =
            GerenciadorComodos.Instance;

        int numeroComodo = gerenciador.NumeroComodoAtual;

        if (!NumeroComodoValido(numeroComodo))
            return;

        Color cor = gerenciador.comodos[numeroComodo - 1].cor;

        EnviarCorComodo(
            numeroComodo,
            cor,
            intensidade
        );
    }

    // =====================================================
    // CONTROLE GLOBAL: TODAS AS LÂMPADAS
    // =====================================================

    public void LigarLED()
    {
        EnviarComando("LED_ON");
    }

    public void DesligarLED()
    {
        EnviarComando("LED_OFF");
    }

    //SENSOR DE MOVIMENTO
    
    public void ConfigurarSensorMovimento(bool ativo)
    {
        if (ativo)
        {
            EnviarComando("MOTION_ON");
            Debug.Log("Sensor de movimento ativado.");
        }
        else
        {
           EnviarComando("MOTION_OFF");
           Debug.Log("Sensor de movimento desativado.");
        }
    }


    // =====================================================
    // ENVIAR COMANDO TCP
    // =====================================================

    
public async void EnviarComando(string comando)
{
    if (!ativo)
        return;

    await travaEnvio.WaitAsync();

    try
    {
        // Aguarda a conexão inicial ou uma reconexão.
        int espera = 0;

        while (
            ativo &&
            (!conectado || cliente == null || stream == null) &&
            espera < 15000
        )
        {
            await Task.Delay(100);
            espera += 100;
        }

        if (
            !ativo ||
            !conectado ||
            cliente == null ||
            !cliente.Connected ||
            stream == null
        )
        {
            Debug.LogWarning(
                "ESP32 não conectado. Comando não enviado: " + comando
            );

            return;
        }

        byte[] dados = Encoding.UTF8.GetBytes(comando + "\n");

        await stream.WriteAsync(dados, 0, dados.Length);

        Debug.Log("Comando enviado: " + comando);
    }
    catch (Exception erro)
    {
        Debug.LogWarning(
            "Erro ao enviar comando: " + erro.Message
        );

        Desconectar();
    }
    finally
    {
        travaEnvio.Release();
    }
}


    // =====================================================
    // DESCONECTAR
    // =====================================================

    private void Desconectar()
    {
        conectado = false;

        if (stream != null)
        {
            try
            {
                stream.Close();
            }
            catch
            {
                // Ignora erros ao fechar o fluxo.
            }

            stream = null;
        }

        if (cliente != null)
        {
            try
            {
                cliente.Close();
            }
            catch
            {
                // Ignora erros ao fechar o cliente.
            }

            cliente = null;
        }

        Debug.Log("Conexão com ESP32 encerrada.");
    }

    // =====================================================
    // ENCERRAMENTO
    // =====================================================

    private void OnDestroy()
    {
        ativo = false;
        Desconectar();
    }
}
