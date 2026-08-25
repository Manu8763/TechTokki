using UnityEngine;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

public class ESP32Controller : MonoBehaviour
{
    private string ip = "";

    private int porta = 8080;

    private const int discoveryPort = 4210;

    private UdpClient udp;


    async void Start()
    {
        await ProcurarESP32();
    }


    async Task ProcurarESP32()
    {
        Debug.Log("Procurando ESP32...");

        try
        {
            udp = new UdpClient();

            udp.EnableBroadcast = true;

            byte[] mensagem =
                Encoding.UTF8.GetBytes("ESP32_DISCOVERY");


            IPEndPoint destino =
                new IPEndPoint(
                    IPAddress.Broadcast,
                    discoveryPort
                );


            udp.Send(
                mensagem,
                mensagem.Length,
                destino
            );


            Task<UdpReceiveResult> recebimento =
                udp.ReceiveAsync();


            if (await Task.WhenAny(
                recebimento,
                Task.Delay(3000)
            ) != recebimento)
            {
                Debug.LogError(
                    "Nenhum ESP32 encontrado."
                );

                udp.Close();

                return;
            }


            UdpReceiveResult resultado =
                await recebimento;


            string resposta =
                Encoding.UTF8.GetString(
                    resultado.Buffer
                );


            Debug.Log(
                "Resposta do ESP32: " +
                resposta
            );


            string[] partes =
                resposta.Split('|');


            if (partes.Length >= 3 &&
                partes[0] == "ESP32")
            {
                ip = partes[1];

                porta =
                    int.Parse(partes[2]);


                Debug.Log(
                    "ESP32 encontrado!"
                );

                Debug.Log(
                    "IP: " + ip
                );

                Debug.Log(
                    "Porta: " + porta
                );
            }


            udp.Close();
        }
        catch (System.Exception erro)
        {
            Debug.LogError(
                "Erro na descoberta: " +
                erro.Message
            );
        }
    }


    public void LigarLED()
    {
        EnviarComando("LED_ON");
    }


    public void DesligarLED()
    {
        EnviarComando("LED_OFF");
    }


    async void EnviarComando(string comando)
    {
        if (string.IsNullOrEmpty(ip))
        {
            Debug.LogError(
                "ESP32 ainda não foi encontrado!"
            );

            return;
        }


        try
        {
            using (TcpClient cliente =
                new TcpClient())
            {

                Task conexao =
                    cliente.ConnectAsync(
                        ip,
                        porta
                    );


                if (await Task.WhenAny(
                    conexao,
                    Task.Delay(2000)
                ) != conexao)
                {
                    Debug.LogError(
                        "Não foi possível conectar ao ESP32."
                    );

                    return;
                }


                NetworkStream stream =
                    cliente.GetStream();


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
        }
        catch (System.Exception erro)
        {
            Debug.LogError(
                "Erro ao enviar comando: " +
                erro.Message
            );
        }
    }


    private void OnDestroy()
    {
        if (udp != null)
        {
            udp.Close();
        }
    }
}