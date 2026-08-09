using UnityEngine;
using System.Net.Sockets;
using System.Text;


public class ESP32Controller : MonoBehaviour
{

    string ip = "10.22.134.200";

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


        TcpClient cliente =
            new TcpClient(ip, porta);



        NetworkStream stream =
            cliente.GetStream();



        byte[] dados =
            Encoding.UTF8.GetBytes(comando + "\n");



        stream.Write(
            dados,
            0,
            dados.Length
        );


        cliente.Close();

    }

}