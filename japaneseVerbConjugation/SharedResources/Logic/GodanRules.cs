using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JapaneseVerbConjugation.SharedResources.Logic
{
    internal static class GodanRules
    {
        public static string TeEnding(char lastKana) => lastKana switch
        {
            'う' or 'つ' or 'る' => "って",
            'む' or 'ぶ' or 'ぬ' => "んで",
            'く' => "いて",
            'ぐ' => "いで",
            'す' => "して",
            _ => ""
        };

        public static string PastEnding(char lastKana) => lastKana switch
        {
            'う' or 'つ' or 'る' => "った",
            'む' or 'ぶ' or 'ぬ' => "んだ",
            'く' => "いた",
            'ぐ' => "いだ",
            'す' => "した",
            _ => ""
        };

        public static string IStem(char lastKana) => lastKana switch
        {
            'う' => "い",
            'つ' => "ち",
            'る' => "り",
            'む' => "み",
            'ぶ' => "び",
            'ぬ' => "に",
            'く' => "き",
            'ぐ' => "ぎ",
            'す' => "し",
            _ => ""
        };

        // "a-stem" used for negative/passive/causative; crucial: う -> わ
        public static string AStem(char lastKana) => lastKana switch
        {
            'う' => "わ",
            'つ' => "た",
            'る' => "ら",
            'む' => "ま",
            'ぶ' => "ば",
            'ぬ' => "な",
            'く' => "か",
            'ぐ' => "が",
            'す' => "さ",
            _ => ""
        };

        public static string EStem(char lastKana) => lastKana switch
        {
            'う' => "え",
            'つ' => "て",
            'る' => "れ",
            'む' => "め",
            'ぶ' => "べ",
            'ぬ' => "ね",
            'く' => "け",
            'ぐ' => "げ",
            'す' => "せ",
            _ => ""
        };

        public static string OStem(char lastKana) => lastKana switch
        {
            'う' => "お",
            'つ' => "と",
            'る' => "ろ",
            'む' => "も",
            'ぶ' => "ぼ",
            'ぬ' => "の",
            'く' => "こ",
            'ぐ' => "ご",
            'す' => "そ",
            _ => ""
        };
    }
}
