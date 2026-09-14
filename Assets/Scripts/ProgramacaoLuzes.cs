using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

public class ProgramacaoLuzes : MonoBehaviour
{
    // =====================================================
    // UI
    // =====================================================

    public Toggle toggleProgramacao;

    public TMP_InputField horarioLigar;
    public TMP_InputField horarioDesligar;

    // =====================================================
    // ESP32
    // =====================================================

    public ESP32Controller esp32;

    // =====================================================
    // START
    // =====================================================

    void Start()
    {
        toggleProgramacao.onValueChanged.AddListener(
            AlterarProgramacao
        );

        horarioLigar.onEndEdit.AddListener(
            AlterarHorario
        );

        horarioDesligar.onEndEdit.AddListener(
            AlterarHorario
        );
    }

    // =====================================================
    // TOGGLE
    // =====================================================

    void AlterarProgramacao(bool ativo)
    {
        if (ativo)
        {
            Debug.Log(
                "Programação ATIVADA."
            );

            EnviarProgramacao();
        }
        else
        {
            Debug.Log(
                "Programação DESATIVADA."
            );

            esp32.CancelarProgramacao();
        }
    }

    // =====================================================
    // HORARIO ALTERADO
    // =====================================================

    void AlterarHorario(string texto)
    {
        if (!toggleProgramacao.isOn)
            return;

        EnviarProgramacao();
    }

    // =====================================================
    // ENVIAR PROGRAMAÇÃO
    // =====================================================

    void EnviarProgramacao()
    {
        string ligar =
            horarioLigar.text.Trim();

        string desligar =
            horarioDesligar.text.Trim();

        // Verificar se os dois horários existem

        if (
            string.IsNullOrEmpty(ligar) ||
            string.IsNullOrEmpty(desligar)
        )
        {
            Debug.LogWarning(
                "Preencha os dois horários."
            );

            return;
        }

        // Verificar formato

        if (!HorarioValido(ligar))
        {
            Debug.LogWarning(
                "Horário de ligar inválido."
            );

            return;
        }

        if (!HorarioValido(desligar))
        {
            Debug.LogWarning(
                "Horário de desligar inválido."
            );

            return;
        }

        Debug.Log(
            "Enviando programação:"
        );

        Debug.Log(
            "Ligar: " + ligar
        );

        Debug.Log(
            "Desligar: " + desligar
        );

        esp32.ProgramarLuzes(
            ligar,
            desligar
        );
    }

    // =====================================================
    // VALIDAR HORARIO
    // =====================================================

    bool HorarioValido(string horario)
    {
        DateTime resultado;

        return DateTime.TryParseExact(
            horario,
            "HH:mm",
            null,
            System.Globalization.DateTimeStyles.None,
            out resultado
        );
    }
}