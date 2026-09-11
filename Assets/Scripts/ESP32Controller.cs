using UnityEngine;
using System.Net;
using System.Net.Sockets;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Generic;

public class ESP32Controller : MonoBehaviour
{
    // =====================================================
    // CONFIGURAÇÃO
    // =====================================================

    private const int portaTCP = 8080;

    // =====================================================
    // CONEXÃO
    // =====================================================

    private TcpClient cliente;
    private NetworkStream stream;

    private bool conectado = false;
    private bool conectando = false;

    private string enderecoESP32 = null;

    // =====================================================
    // CONTROLE DE COR
    // =====================================================

    private bool corAtiva = false;
    private Color ultimaCor = Color.white;

    // =====================================================
    // START
    // =====================================================

    async void Start()
    {
        Debug.Log("=================================");
        Debug.Log("ESP32 CONTROLLER INICIADO");
        Debug.Log("=================================");

        await ConectarContinuamente();
    }

    // =====================================================
    // TENTAR CONECTAR CONTINUAMENTE
    // =====================================================

    async Task ConectarContinuamente()
    {
        while (true)
        {
            if (!conectado && !conectando)
            {
                await DescobrirEConectar();
            }

            await Task.Delay(2000);
        }
    }

    // =====================================================
    // DESCOBRIR ESP32
    // =====================================================

    async Task<string> DescobrirESP32()
    {
        Debug.Log("=================================");
        Debug.Log("PROCURANDO ESP32...");
        Debug.Log("=================================");

        List<string> enderecos = ObterEnderecosDaRede();

        if (enderecos.Count == 0)
        {
            Debug.LogWarning(
                "Não foi possível descobrir a rede local."
            );

            return null;
        }

        Debug.Log(
            "IPs encontrados para testar: " +
            enderecos.Count
        );

        // Testa vários IPs simultaneamente
        List<Task<string>> tarefas =
            new List<Task<string>>();

        foreach (string ip in enderecos)
        {
            tarefas.Add(TestarIP(ip));
        }

        while (tarefas.Count > 0)
        {
            Task<string> tarefa =
                await Task.WhenAny(tarefas);

            tarefas.Remove(tarefa);

            string resultado = await tarefa;

            if (!string.IsNullOrEmpty(resultado))
            {
                // Cancela conceitualmente o restante
                // simplesmente ignorando os resultados.

                Debug.Log(
                    "================================="
                );

                Debug.Log(
                    "ESP32 ENCONTRADO!"
                );

                Debug.Log(
                    "IP: " + resultado
                );

                Debug.Log(
                    "================================="
                );

                return resultado;
            }
        }

        Debug.LogWarning(
            "Nenhum ESP32 encontrado na rede."
        );

        return null;
    }

    // =====================================================
    // OBTER IPS DA REDE LOCAL
    // =====================================================

    List<string> ObterEnderecosDaRede()
    {
        List<string> resultado =
            new List<string>();

        foreach (
            NetworkInterface rede
            in NetworkInterface.GetAllNetworkInterfaces()
        )
        {
            if (
                rede.OperationalStatus !=
                OperationalStatus.Up
            )
            {
                continue;
            }

            if (
                rede.NetworkInterfaceType ==
                NetworkInterfaceType.Loopback
            )
            {
                continue;
            }

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
                {
                    continue;
                }

                IPAddress ip =
                    endereco.Address;

                IPAddress mascara =
                    endereco.IPv4Mask;

                if (mascara == null)
                    continue;

                byte[] ipBytes =
                    ip.GetAddressBytes();

                byte[] mascaraBytes =
                    mascara.GetAddressBytes();

                byte[] redeBytes =
                    new byte[4];

                for (int i = 0; i < 4; i++)
                {
                    redeBytes[i] =
                        (byte)(
                            ipBytes[i] &
                            mascaraBytes[i]
                        );
                }

                int primeiro =
                    EnderecoParaInt(redeBytes);

                int mascaraInt =
                    EnderecoParaInt(mascaraBytes);

                int broadcast =
                    primeiro |
                    (~mascaraInt);

                // Evitar testar a rede e broadcast
                int inicio = primeiro + 1;
                int fim = broadcast - 1;

                // Segurança: não varrer redes gigantes
                if (fim - inicio > 1024)
                {
                    Debug.LogWarning(
                        "Rede muito grande. " +
                        "Ignorando interface: " +
                        ip
                    );

                    continue;
                }

                for (
                    int numero = inicio;
                    numero <= fim;
                    numero++
                )
                {
                    resultado.Add(
                        IntParaEndereco(numero)
                    );
                }

                Debug.Log(
                    "Rede detectada: " +
                    ip +
                    " / " +
                    mascara
                );
            }
        }

        return resultado;
    }

    // =====================================================
    // CONVERTER IP → INT
    // =====================================================

    int EnderecoParaInt(byte[] bytes)
    {
        return
            (bytes[0] << 24) |
            (bytes[1] << 16) |
            (bytes[2] << 8) |
            bytes[3];
    }

    // =====================================================
    // CONVERTER INT → IP
    // =====================================================

    string IntParaEndereco(int valor)
    {
        byte a =
            (byte)((valor >> 24) & 255);

        byte b =
            (byte)((valor >> 16) & 255);

        byte c =
            (byte)((valor >> 8) & 255);

        byte d =
            (byte)(valor & 255);

        return $"{a}.{b}.{c}.{d}";
    }

    // =====================================================
    // TESTAR UM IP
    // =====================================================

    async Task<string> TestarIP(string ip)
    {
        try
        {
            TcpClient teste =
                new TcpClient();

            Task conexao =
                teste.ConnectAsync(
                    ip,
                    portaTCP
                );

            Task terminado =
                await Task.WhenAny(
                    conexao,
                    Task.Delay(500)
                );

            if (terminado == conexao &&
                teste.Connected)
            {
                teste.Close();

                Debug.Log(
                    "Servidor encontrado em: " +
                    ip
                );

                return ip;
            }

            teste.Close();
        }
        catch
        {
            // IP não possui ESP32.
        }

        return null;
    }

    // =====================================================
    // DESCOBRIR E CONECTAR
    // =====================================================

    async Task DescobrirEConectar()
    {
        if (conectando)
            return;

        conectando = true;

        try
        {
            enderecoESP32 =
                await DescobrirESP32();

            if (
                string.IsNullOrEmpty(
                    enderecoESP32
                )
            )
            {
                conectando = false;
                return;
            }

            await Conectar();
        }
        catch (
            System.Exception erro
        )
        {
            Debug.LogWarning(
                "Erro na conexão: " +
                erro.Message
            );
        }

        conectando = false;
    }

    // =====================================================
    // CONECTAR TCP
    // =====================================================

    async Task Conectar()
    {
        if (
            string.IsNullOrEmpty(
                enderecoESP32
            )
        )
        {
            return;
        }

        Debug.Log(
            "Conectando ao ESP32 em " +
            enderecoESP32 +
            ":" +
            portaTCP
        );

        try
        {
            TcpClient novoCliente =
                new TcpClient();

            Task conexao =
                novoCliente.ConnectAsync(
                    enderecoESP32,
                    portaTCP
                );

            Task terminado =
                await Task.WhenAny(
                    conexao,
                    Task.Delay(3000)
                );

            if (terminado != conexao)
            {
                novoCliente.Close();

                Debug.LogWarning(
                    "Tempo esgotado ao conectar."
                );

                return;
            }

            if (!novoCliente.Connected)
            {
                novoCliente.Close();

                Debug.LogWarning(
                    "Não foi possível conectar."
                );

                return;
            }

            cliente =
                novoCliente;

            stream =
                cliente.GetStream();

            conectado = true;

            Debug.Log(
                "================================="
            );

            Debug.Log(
                "ESP32 CONECTADO COM SUCESSO!"
            );

            Debug.Log(
                "IP: " +
                enderecoESP32
            );

            Debug.Log(
                "================================="
            );
        }
        catch (
            System.Exception erro
        )
        {
            Debug.LogWarning(
                "Erro ao conectar: " +
                erro.Message
            );

            Desconectar();
        }
    }

    // =====================================================
    // TOGGLE DE COR
    // =====================================================

    public void AlternarCor(bool ativo)
    {
        corAtiva = ativo;

        Debug.Log(
            "Controle de cor: " +
            ativo
        );

        if (corAtiva)
        {
            EnviarCor(ultimaCor);
        }
        else
        {
            EnviarComando(
                "RGB:255,255,255"
            );
        }
    }

    // =====================================================
    // ENVIAR COR
    // =====================================================

    public void EnviarCor(Color cor)
    {
        if (!corAtiva)
        {
            Debug.Log(
                "COR NÃO ENVIADA: Toggle está DESATIVADO."
            );

            return;
        }

        ultimaCor = cor;

        int r =
            Mathf.RoundToInt(cor.r * 255);

        int g =
            Mathf.RoundToInt(cor.g * 255);

        int b =
            Mathf.RoundToInt(cor.b * 255);

        Debug.Log(
            $"ENVIANDO RGB: {r},{g},{b}"
        );

        EnviarComando(
            $"RGB:{r},{g},{b}"
        );
    }

    // =====================================================
    // LIGAR LED
    // =====================================================

    public void LigarLED()
    {
    // Primeiro liga fisicamente a lâmpada.
    EnviarComando("LED_ON");

    // Se o controle de cor estiver ativo,
    // restaura a última cor escolhida.
    if (corAtiva)
    {
        EnviarCor(ultimaCor);
    }
    }

    // =====================================================
    // DESLIGAR LED
    // =====================================================

    public void DesligarLED()
    {
        EnviarComando(
            "LED_OFF"
        );
    }

    // =====================================================
    // ENVIAR COMANDO
    // =====================================================

    async void EnviarComando(
        string comando
    )
    {
        if (
            !conectado ||
            cliente == null ||
            !cliente.Connected ||
            stream == null
        )
        {
            Debug.LogWarning(
                "ESP32 não está conectado."
            );

            return;
        }

        try
        {
            byte[] dados =
                Encoding.UTF8.GetBytes(
                    comando + "\n"
                );

            await stream.WriteAsync(
                dados,
                0,
                dados.Length
            );

            Debug.Log(
                "Comando enviado: " +
                comando
            );
        }
        catch (
            System.Exception erro
        )
        {
            Debug.LogWarning(
                "Erro ao enviar comando: " +
                erro.Message
            );

            Desconectar();
        }
    }

    // =====================================================
    // DESCONECTAR
    // =====================================================

    void Desconectar()
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
            }

            cliente = null;
        }

        Debug.Log(
            "Conexão com ESP32 encerrada."
        );
    }

    // =====================================================
    // DESTROY
    // =====================================================

    void OnDestroy()
    {
        Desconectar();
    }
}