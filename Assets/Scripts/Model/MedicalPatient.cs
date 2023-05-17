using UnityEngine;

public class MedicalPatient : MonoBehaviour
{
    // Properties
    public string Name { get; set; }
    public int Age { get; set; }
    public Gender Gender { get; set; }
    public float Height { get; set; } // Height in meters
    public float Weight { get; set; } // Weight in kilograms
    public BloodType BloodType { get; set; }
    public string MedicalHistory { get; set; }
    public bool IsSmoker { get; set; }
    public bool HasAllergies { get; set; }
    public bool HasHeartCondition { get; set; }
    public bool IsPregnant { get; set; }
    public string CurrentMedications { get; set; }
    public float BodyTemperature { get; set; } // Body temperature in Celsius
    public int HeartRate { get; set; } // Heart rate in beats per minute
    public int RespiratoryRate { get; set; } // Respiratory rate in breaths per minute
    public float OxygenSaturation { get; set; } // Oxygen saturation level as a percentage

    // Constants for blood pressure calculation
    private const float SystolicConstant = 109;
    private const float DiastolicConstant = 63;

    // Methods
    public void CalculateBloodPressure(out int systolic, out int diastolic)
    {
        float bmi = CalculateBMI();
        systolic = Mathf.RoundToInt(SystolicConstant + (bmi * 0.15f));
        diastolic = Mathf.RoundToInt(DiastolicConstant + (bmi * 0.1f));
    }

    public void SetVitalSigns(float bodyTemp, int heartRate, int respiratoryRate, float oxygenSaturation)
    {
        BodyTemperature = bodyTemp;
        HeartRate = heartRate;
        RespiratoryRate = respiratoryRate;
        OxygenSaturation = oxygenSaturation;
    }

    public void AddMedication(string medication)
    {
        CurrentMedications += medication + "\n";
    }

    public void ClearMedications()
    {
        CurrentMedications = string.Empty;
    }

    public void UpdateHeight(float height)
    {
        Height = height;
    }

    public void UpdateWeight(float weight)
    {
        Weight = weight;
    }

    private float CalculateBMI()
    {
        return Weight / (Height * Height);
    }
}

// Enumeration for gender
public enum Gender
{
    Male,
    Female
}

// Enumeration for blood type
public enum BloodType
{
    APositive,
    ANegative,
    BPositive,
    BNegative,
    ABPositive,
    ABNegative,
    OPositive,
    ONegative
}