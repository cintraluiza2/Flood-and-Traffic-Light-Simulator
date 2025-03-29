using System;
using System.Net.Sockets;
using System.Text;
using UnityEngine;
using System.Collections;

public class TrafficTest : MonoBehaviour
{
    public GameObject redLight;
    public GameObject yellowLight;
    public GameObject greenLight;

    string server = "...";
    int port = ...;

    private void Start()
    {
        StartCoroutine(LightLoop());
    }

    IEnumerator LightLoop()
    {
        while (true)
        {
            CheckTrafficLight();
            yield return new WaitForSeconds(2f); // espera 2 segundos antes de repetir
        }
    }

    void CheckTrafficLight()
    {
        try
        {
            using (TcpClient client = new TcpClient(server, port))
            using (NetworkStream stream = client.GetStream())
            {
                Debug.Log("Conectado ao servidor.");

                string message = "Olá, servidor!";
                byte[] data = Encoding.UTF8.GetBytes(message);
                stream.Write(data, 0, data.Length);
                Debug.Log($"Mensagem enviada: {message}");

                byte[] buffer = new byte[1024];
                int bytesRead = stream.Read(buffer, 0, buffer.Length);
                string response = Encoding.UTF8.GetString(buffer, 0, bytesRead);

                Debug.Log($"Mensagem recebida do servidor: {response}");

                if (response == "VERMELHO")
                    SetLightState(true, false, false);
                else if (response == "AMARELO")
                    SetLightState(false, true, false);
                else if (response == "VERDE")
                    SetLightState(false, false, true);
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"Erro: {e.Message}");
        }
    }

    private void SetLightState(bool red, bool yellow, bool green)
    {
        redLight.SetActive(red);
        yellowLight.SetActive(yellow);
        greenLight.SetActive(green);
    }
}
