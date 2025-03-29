using UnityEngine;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System;
using Newtonsoft.Json.Linq;

[System.Serializable]
public class SetorAgua
{
    public string nome;
    public Transform objeto;
    public float velocidadeEscoamento;
}

public class EscoamentoAgua : MonoBehaviour
{
    [Header("Configuração Global")]
    public float intervalo = 15f; // Tempo entre as quedas
    
    [Header("Setores de Água")]
    public SetorAgua[] setores = new SetorAgua[4];
    
    private float tempoAtual = 0f;
    private Thread socketThread;
    private bool rodando = true;
    private string dadosRecebidos = "";

    void Start()
    {
        socketThread = new Thread(ReceberDadosSocket);
        socketThread.IsBackground = true;
        socketThread.Start();
    }

    void Update()
    {
        tempoAtual += Time.deltaTime;
        
        if (!string.IsNullOrEmpty(dadosRecebidos))
        {
            AtualizarVelocidades(dadosRecebidos);
            dadosRecebidos = "";
        }
        
        if (tempoAtual >= intervalo)
        {
            foreach (SetorAgua setor in setores)
            {
                Escoar(setor);
            }
            tempoAtual = 0f;
        }
    }

    void Escoar(SetorAgua setor)
    {
        if (setor.objeto != null)
        {
            Vector3 novaPos = setor.objeto.position;
            novaPos.y -= setor.velocidadeEscoamento;
            setor.objeto.position = novaPos;
            
            Debug.Log($"Setor '{setor.nome}' nova posição Y: {setor.objeto.position.y}");
        }
    }

    void ReceberDadosSocket()
    {
        try
        {
            TcpClient client = new TcpClient("...", ...);
            NetworkStream stream = client.GetStream();
            byte[] buffer = new byte[1024];
            
            while (rodando)
            {
                int bytesRead = stream.Read(buffer, 0, buffer.Length);
                if (bytesRead > 0)
                {
                    string mensagem = Encoding.UTF8.GetString(buffer, 0, bytesRead);
                    dadosRecebidos = mensagem;
                }
            }
            
            client.Close();
        }
        catch (Exception ex)
        {
            Debug.LogError("Erro ao conectar ao servidor: " + ex.Message);
        }
    }

    void AtualizarVelocidades(string json)
    {
        try
        {
            JObject dados = JObject.Parse(json);
            
            setores[0].velocidadeEscoamento = (float)dados["area_a"] * 0.001f;
            setores[1].velocidadeEscoamento = (float)dados["area_b"] * 0.001f;
            setores[2].velocidadeEscoamento = (float)dados["area_c"] * 0.001f;
            setores[3].velocidadeEscoamento = (float)dados["area_d"] * 0.001f;
            
            Debug.Log("Velocidades de escoamento atualizadas!");
        }
        catch (Exception ex)
        {
            Debug.LogError("Erro ao processar JSON: " + ex.Message);
        }
    }

    private void OnApplicationQuit()
    {
        rodando = false;
        socketThread.Abort();
    }
}
