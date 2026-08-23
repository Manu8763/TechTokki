using UnityEngine;
using System.Net;
using System.Net.Sockets;
using System.Text;

public class ESP32Controller : MonoBehaviour
{
    string host = "esp32.local";
    int porta = 8080;

    public void LigarLED()
    {
        EnviarComando("LED_ON");
    }

    public void DesligarLED()
    {
        EnviarComando("LED_OFF");
    }

    void EnviarComando(string comando)
    {
        try
        {
            Debug.Log("Procurando ESP32...");

            IPHostEntry hostEntry = Dns.GetHostEntry(host);

            IPAddress ip = hostEntry.AddressList[0];

            Debug.Log("ESP32 encontrado em: " + ip);

            using (TcpClient cliente = new TcpClient(ip.ToString(), porta))
            {
                NetworkStream stream = cliente.GetStream();

                byte[] dados = Encoding.UTF8.GetBytes(comando + "\n");

                stream.Write(dados, 0, dados.Length);

                Debug.Log("Comando enviado: " + comando);
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError("Erro ao conectar ao ESP32: " + e.Message);
        }
    }
}