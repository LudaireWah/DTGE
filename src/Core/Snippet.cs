using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace DtgeCore;

/**
 * DTGE's Scenes are made up of a sequence of snippets, with each snippet containing
 * the desired text and any business logic of how the scene should unfold and change
 * in response to various kinds of information
 */
 [Serializable]
public class Snippet
{   
    public enum ConditionalMode
    {
        Simple,
        Random,
        Subscene
    }

    public ConditionalMode CurrentConditionalMode
    {
        get; set;
    }

    public List<string> snippetVariations
    {
        get; set;
    }
    private Random snippetRandomizer;
    private readonly int snippetRandomizerSeed;
    private int currentRandomizedVariationIndex;

    public Snippet()
    {
        this.snippetVariations = new List<string>();
        this.snippetVariations.Add(string.Empty);

        Random seedGenerator = new Random();
        this.snippetRandomizerSeed = seedGenerator.Next();
        this.snippetRandomizer = new Random(this.snippetRandomizerSeed);
        this.currentRandomizedVariationIndex = snippetRandomizer.Next(this.snippetVariations.Count);
    }

    public void CopyFrom(Snippet other)
    {
        this.CurrentConditionalMode = other.CurrentConditionalMode;
        this.snippetVariations.Clear();
        for (int variationIndex = 0; variationIndex < other.snippetVariations.Count; variationIndex++)
        {
            this.snippetVariations.Add(other.snippetVariations[variationIndex]);
        }
    }

    public void AddSnippetVariation()
    {
        this.snippetVariations.Add("");
    }

    public void RemoveSnippetVariation(int variationIndex)
    {
        this.snippetVariations.RemoveAt(variationIndex);
    }

    public void SetSnippetVariationText(int snippetTextIndex, string text)
    {
        if (!(this.snippetVariations.Count > snippetTextIndex))
        {
            // error handling
        }
        else
        {
            this.snippetVariations[snippetTextIndex] = text;
        }
    }

    public int GetSnippetVariationCount()
    {
        return this.snippetVariations.Count;
    }

    public string CalculateText(bool preserveRandomization)
    {
        if (!preserveRandomization)
        {
            currentRandomizedVariationIndex = this.snippetRandomizer.Next(this.snippetVariations.Count);
        }
        return this.snippetVariations[currentRandomizedVariationIndex];
    }

    public string GetVariationTextByIndex(int variationIndex)
    {
        string foundSnippetText = null;
        if (variationIndex >= this.snippetVariations.Count)
        {
            // Error
        }
        else
        {
            foundSnippetText = this.snippetVariations[variationIndex];
        }
        return foundSnippetText;
    }

    public void SetRandomizedIndex(int variationIndex)
    {
        this.currentRandomizedVariationIndex = variationIndex;
    }
}
