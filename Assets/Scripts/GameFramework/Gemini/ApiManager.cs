using Uralstech.UGemini;
using Uralstech.UGemini.Chat;
using Uralstech.UGemini.Models;
using UnityEngine;
using System;
using System.Linq;
using System.Threading.Tasks;

public class ApiManager : MonoBehaviour
{
    const int PROBABILITY_TRUE = 30;
    const int PROBABILITY_FALSE = 45;
    const int PROBABILITY_UNKNOWN = 25;

    private void Start()
    {
        // QueryGemini("CH1", "CH2", new string[] { "CH3", "CH4" });
    }

    public async Task<string> QueryGemini(string killer, string victum, string[] witnesses)
    {
        string witnessNames = String.Join(" ve ", witnesses);

        string witnessText =
            witnessNames.Length > 0 ? $" O sırada {witnessNames} de oradan geçiyordu." : "";
        string witnessExtension =
            witnessNames.Length > 0 ? $" ve suçu {witnessNames}'e atabilirsin" : "";

        string text =
            $"ortaçağ temalı bir oyunda köylüsün. {killer} insan kılığına girebilen bir hayalet ve {killer}'in {victum}'yi öldürdüğünü gördün.{witnessText} MAIN_PLAYER geldi ve sana 'burda neler oldu?' dedi. %{PROBABILITY_TRUE} ihtimalle Gerçeği söyleyebilirsin, %{PROBABILITY_FALSE} ihtimalle yalan söyleyebilirsin{witnessExtension} veya %{PROBABILITY_UNKNOWN} ihtimalle belirsiz cevap verebilirsin. tek bir karar ver. sadece cevabını söyle. alternatif sonuçlardan veya diğer seçeneklerden bahsetme. MAIN_PLAYER'e ne söylerdin? cümlelerini hikayeleştir ve en az 4 cümle, en fazla 6 cümle kur.";

        GeminiChatResponse response = await GeminiManager.Instance.Request<GeminiChatResponse>(
            new GeminiChatRequest(GeminiModel.Gemini1_5Flash)
            {
                Contents = new GeminiContent[] { GeminiContent.GetContent(text, GeminiRole.User), },
            }
        );

        return response.Parts[0].Text;
    }

    // public async void QueryGemini(string killer, string victum, string[] witnesses)
    // {
    //     string witnessNames = String.Join(" ve ", witnesses);

    //     string witnessText =
    //         witnessNames.Length > 0 ? $" O sırada {witnessNames} de oradan geçiyordu." : "";
    //     string witnessExtension =
    //         witnessNames.Length > 0 ? $" ve suçu {witnessNames}'e atabilirsin" : "";

    //     string text =
    //         $"ortaçağ temalı bir oyunda köylüsün. {killer} insan kılığına girebilen bir hayalet ve {killer}'in {victum}'yi öldürdüğünü gördün.{witnessText} MAIN_PLAYER geldi ve sana 'burda neler oldu?' dedi. %{PROBABILITY_TRUE} ihtimalle Gerçeği söyleyebilirsin, %{PROBABILITY_FALSE} ihtimalle yalan söyleyebilirsin{witnessExtension} veya %{PROBABILITY_UNKNOWN} ihtimalle belirsiz cevap verebilirsin. tek bir karar ver. sadece cevabını söyle. alternatif sonuçlardan veya diğer seçeneklerden bahsetme. MAIN_PLAYER'e ne söylerdin? cümlelerini hikayeleştir ve en az 4 cümle, en fazla 6 cümle kur.";

    //     // string text =
    //     //     "ortaçağ temalı bir oyunda köylüsün. CH1 insan kılığına girebilen bir hayalet ve CH1'in CH2'yi öldürdüğünü gördün. O sırada CH3 de oradan geçiyordu. CH4 geldi ve sana 'burda neler oldu?' dedi. %30 ihtimalle Gerçeği söyleyebilirsin, %45 ihtimalle yalan söyleyebilir ve suçu CH3'e atabilirsin veya %25 ihtimalle belirsiz cevap verebilirsin. tek bir karar ver. sadece cevabını söyle. alternatif sonuçlardan veya diğer seçeneklerden bahsetme. CH4'e ne söylerdin? cümlelerini hikayeleştir ve en az 4 cümle, en fazla 6 cümle kur.";

    //     GeminiChatResponse response = await GeminiManager.Instance.Request<GeminiChatResponse>(
    //         new GeminiChatRequest(GeminiModel.Gemini1_5Flash)
    //         {
    //             Contents = new GeminiContent[] { GeminiContent.GetContent(text, GeminiRole.User), },
    //         }
    //     );

    //     Debug.Log(response.Parts[0].Text);
    // }
}
