using UnityEngine;

[System.Serializable]
public class DadosComodo
{
    public string nome;

    [Header("Luz")]
    public bool luzLigada = false;
    [Range(0, 100)]
    public float intensidade = 100f;
    public Color cor = Color.white;

    [Header("Programação")]
    public bool programacaoAtiva = false;
    public string horarioAcender = "HH:MM";
    public string horarioApagar = "HH:MM";

    [Header("Sensores")]
    public bool sensorMovimento = false;
    public bool sensorLuminosidade = false;
}