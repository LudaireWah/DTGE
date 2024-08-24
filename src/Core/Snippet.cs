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
public class Snippet
{   
    public enum ConditionalMode
    {
        Simple,
        Subscene,
        Random
    }

    public ConditionalMode CurrentConditionalMode;

    public List<string> snippetVariations
    {
        get; set;
    }

    private ISceneContextProvider sceneContextProvider;

    private Random snippetRandomizer;
    private readonly int snippetRandomizerSeed;
    private int currentRandomizedVariationIndex;

    public Snippet()
    {
        this.snippetVariations = new List<string>();
        this.snippetVariations.Add(string.Empty);

        this.sceneContextProvider = null;

        Random seedGenerator = new Random();
        this.snippetRandomizerSeed = seedGenerator.Next();
        this.snippetRandomizer = new Random(this.snippetRandomizerSeed);
        this.currentRandomizedVariationIndex = snippetRandomizer.Next(this.snippetVariations.Count);
    }

    public Snippet(ISceneContextProvider sceneContextProvider)
    {
        this.snippetVariations = new List<string>();
        this.snippetVariations.Add(string.Empty);

        this.sceneContextProvider = sceneContextProvider;

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

    public void SetSceneContextProvider(ISceneContextProvider sceneContextProvider)
    {
        if (this.sceneContextProvider != null)
        {
            // error
        }
        this.sceneContextProvider = sceneContextProvider;
    }

    public void AddVariation()
    {
        this.snippetVariations.Add("");
    }

    public void RemoveVariation(int variationIndex)
    {
        this.snippetVariations.RemoveAt(variationIndex);
    }

    public void SetVariationText(int snippetTextIndex, string text)
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

    public int GetVariationCount()
    {
        int variationCount = 0;
        switch (this.CurrentConditionalMode)
        {
        case ConditionalMode.Simple:
            variationCount = 1;
            break;
        case ConditionalMode.Subscene:
            variationCount = this.sceneContextProvider.GetSubsceneCount();
            break;
        case ConditionalMode.Random:
            variationCount = this.snippetVariations.Count;
            break;
        }
        return variationCount;
    }

    public string CalculateText(bool doNotRandomize)
    {
        string calculatedText = null;

        switch(this.CurrentConditionalMode)
        {
        case ConditionalMode.Simple:
            calculatedText = this.snippetVariations[0];
            break;
        case ConditionalMode.Subscene:
            calculatedText = this.snippetVariations[this.sceneContextProvider.GetCurrentSubsceneIndex()];
            break;
        case ConditionalMode.Random:
            if (!doNotRandomize)
            {
                currentRandomizedVariationIndex = this.snippetRandomizer.Next(this.snippetVariations.Count);
            }
            calculatedText = this.snippetVariations[currentRandomizedVariationIndex];
            break;
        }
        return calculatedText;
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

    public string GetVariationName(int variationIndex)
    {
        string snippetName = null;

        switch(this.CurrentConditionalMode)
        {
        case ConditionalMode.Simple:
            snippetName = "";
            break;
        case ConditionalMode.Subscene:
            snippetName = this.sceneContextProvider.GetSubsceneName(variationIndex);
            break;
        case ConditionalMode.Random:
            snippetName = "Random " + variationIndex;
            break;
        default:
            throw new NotImplementedException();
        }

        return snippetName;
    }

    public bool IsSimpleModeDisabled()
    {
        return this.snippetVariations.Count > 1;
    }

    public bool IsSubsceneModeDisabled()
    {
        return this.sceneContextProvider.GetSubsceneCount() == 0;
    }
}
