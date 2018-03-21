using UnityEngine;

public static class ProceduralGalaxyCaller {

    private static string Vowels = "aeiouy";
    private static string Consonants = "bcdfghjklmnpqrstvwxz";

    public static string GenerateName (int Size)
    {

        string GalaxyName = "";

        bool isVowel = Random.value > .5;

        for (int i = 0; i < Size; i++)
        {

            string NewChar;

            if (isVowel) { NewChar = Vowels[Random.Range(0, Vowels.Length)] + ""; }
            else { NewChar = Consonants[Random.Range(0, Consonants.Length)] + ""; }

            if (i == 0) { NewChar = NewChar.ToUpper(); }

            GalaxyName += NewChar;

            isVowel = !isVowel;

        }

        return GalaxyName;

    }

}
