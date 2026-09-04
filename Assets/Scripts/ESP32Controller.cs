using UnityEngine;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

public class ESP32Controller : MonoBehaviour
{
    // =========================
    // CONFIGURAÇÃO
    // =========================

    private const string enderecoESP32 = "esp32.local";
    private const int porta = 8080;

    // =========================
    // CONEXÃO
    // =========================

    private TcpClient cliente;
    private NetworkStream stream;

    private CancellationTokenSource cancelamento;

    private bool conectado = false;
    private bool conectando = false;

    // =========================
    // CONTROLE DE COR
    // =========================

    private bool corAtiva = false;

    // =========================
    // START
    // =========================

    async void Start()
    {
        cancelamento = new CancellationTokenSource();

        Debug.Log("ESP32 Controller iniciado.");

        await ConectarContinuamente(
            cancelamento.Token
        );
    }

    // =========================
    // CONEXÃO PERMANENTE
    // =========================

    async Task ConectarContinuamente(
        CancellationToken token
    )
    {
        while (!token.IsCancellationRequested)
        {
            if (!conectado && !conectando)
            {
                await Conectar();
            }

            await Task.Delay(1000);
        }
    }

    // =========================
    // CONECTAR
    // =========================

    async Task Conectar()
    {
        if (conectando)
            return;

        conectando = true;

        Debug.Log(
            "Conectando ao ESP32 em " +
            enderecoESP32 +
            ":" +
            porta
        );

        try
        {
            TcpClient novoCliente =
                new TcpClient();

            Task conexao =
                novoCliente.ConnectAsync(
                    enderecoESP32,
                    porta
                );

            if (await Task.WhenAny(
                conexao,
                Task.Delay(3000)
            ) != conexao)
            {
                novoCliente.Close();

                Debug.LogWarning(
                    "Tempo esgotado ao conectar."
                );

                conectando = false;

                return;
            }

            if (!novoCliente.Connected)
            {
                novoCliente.Close();

                Debug.LogWarning(
                    "Não foi possível conectar."
                );

                conectando = false;

                return;
            }

            cliente = novoCliente;
            stream = cliente.GetStream();

            conectado = true;

            Debug.Log(
                "================================="
            );

            Debug.Log(
                "ESP32 CONECTADO!"
            );

            Debug.Log(
                "Conexão permanente estabelecida."
            );

            Debug.Log(
                "================================="
            );
        }
        catch (System.Exception erro)
        {
            Debug.LogWarning(
                "Erro ao conectar: " +
                erro.Message
            );

            Desconectar();
        }

        conectando = false;
    }

    // =========================
    // TOGGLE DE COR
    // =========================

public void AlternarCor(bool ativo)
{
    corAtiva = ativo;

    Debug.Log("Toggle: " + ativo);

    if (!corAtiva)
    {
        EnviarComando("RGB:255,255,255");
    }
}

    // =========================
    // ENVIAR COR
    // =========================

    public void EnviarCor(Color cor)
{
    Debug.Log("EnviarCor foi chamado!");

    if (!corAtiva)
    {
        Debug.Log("COR NÃO ENVIADA: Toggle está DESATIVADO.");
        return;
    }

    int r = Mathf.RoundToInt(cor.r * 255);
    int g = Mathf.RoundToInt(cor.g * 255);
    int b = Mathf.RoundToInt(cor.b * 255);

    Debug.Log($"ENVIANDO RGB: {r}, {g}, {b}");

    EnviarComando($"RGB:{r},{g},{b}");
}

    // =========================
    // LIGAR LED
    // =========================

    public void LigarLED()
    {
        EnviarComando("LED_ON");
    }

    // =========================
    // DESLIGAR LED
    // =========================

    public void DesligarLED()
    {
        EnviarComando("LED_OFF");
    }

    // =========================
    // ENVIAR COMANDO
    // =========================

    async void EnviarComando(
        string comando
    )
    {
        if (!conectado ||
            cliente == null ||
            !cliente.Connected ||
            stream == null)
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
        catch (System.Exception erro)
        {
            Debug.LogWarning(
                "Erro ao enviar comando: " +
                erro.Message
            );

            Desconectar();
        }
    }

    // =========================
    // DESCONECTAR
    // =========================

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

    // =========================
    // DESTROY
    // =========================

    void OnDestroy()
    {
        if (cancelamento != null)
        {
            cancelamento.Cancel();
            cancelamento.Dispose();
        }

        Desconectar();
    }
}