using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Controls the stat sliders
/// </summary>
public class StatBarController : MonoBehaviour
{
    /// <summary>
    /// The slider for the hunger
    /// </summary>
    public Slider HungerSlider;

    /// <summary>
    /// The slider for the thirst
    /// </summary>
    public Slider ThirstSlider;

    /// <summary>
    /// The slider for when they will mate the higher the more likely (they'll still prioritize staying alive)
    /// </summary>
    public Slider HorninessSlider;

    //-------------------------------------------------------------------------------
    /// <summary>
    /// Set the hunger slider's max value
    /// </summary>
    /// <param name="maxValue">The value to set it as max</param>
    public void SetMaxHungerValue(float maxValue)
    {
        HungerSlider.maxValue = maxValue;
    }

    //-----------------------------------------------------------------------------
    /// <summary>
    /// Set the thirst slider's max value
    /// </summary>
    /// <param name="maxValue">The value to set it as max</param>
    public void SetMaxThirstValue(float maxValue)
    {
        ThirstSlider.maxValue = maxValue;
    }

    //----------------------------------------------------------------------------
    /// <summary>
    /// Set the horniness slider's max value
    /// </summary>
    /// <param name="maxValue">The value to set it as max</param>
    public void SetMaxHornyValue(float maxValue)
    {
        HorninessSlider.maxValue = maxValue;
    }

    //------------------------------------------------------------------------------
    /// <summary>
    /// Updates the sliders value
    /// </summary>
    /// <param name="hungerValue">How hungry it is</param>
    /// <param name="thirstValue">How thirsty it is</param>
    /// <param name="hornyValue">How horny it is</param>
    public void UpdateSliders(float hungerValue, float thirstValue, float hornyValue)
    {
        this.HungerSlider.value = hungerValue;
        this.ThirstSlider.value = thirstValue;
        this.HorninessSlider.value = hornyValue;
    }
}