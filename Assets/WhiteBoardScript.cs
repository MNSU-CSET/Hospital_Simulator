using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class WhiteBoardScript : MonoBehaviour
{
    public Scene1Manager sceneScript;

    [Header("CheckPoint #1")]
    public TextMeshPro handsWashed;
    public TextMeshPro introDucedSelf;
    public TextMeshPro confirmID;
    public TextMeshPro startHeadtoToe;

    [Header("CheckPoint #2")]
    public TextMeshPro finishHeadtoToe;
    public TextMeshPro AppliedOxygen;
    public TextMeshPro AssessIV;
    public TextMeshPro SpeakWithFamily;

    [Header("CheckPoint #3")]
    public TextMeshPro AssessPain;
    public TextMeshPro AssessWound;
    public TextMeshPro WoundCulture;

    [Header("CheckPoint #4")]
    public TextMeshPro AdminsteredCam;
    public TextMeshPro NotifiedPhysician;

    // Update is called once per frame
    void Update()
    {
        //checkpoint 1
        if (sceneScript.HandsWashed && handsWashed.fontStyle != FontStyles.Strikethrough) { handsWashed.fontStyle = FontStyles.Strikethrough; }
        if (sceneScript.IntroducedSelf && introDucedSelf.fontStyle != FontStyles.Strikethrough) { introDucedSelf.fontStyle = FontStyles.Strikethrough; }
        if (sceneScript.ConfirmedPatientID && confirmID.fontStyle != FontStyles.Strikethrough) { confirmID.fontStyle = FontStyles.Strikethrough; }
        if (sceneScript.HeadToToeAssesmentBegan && startHeadtoToe.fontStyle != FontStyles.Strikethrough) { startHeadtoToe.fontStyle = FontStyles.Strikethrough; }

        //checkpoint 2
        if (sceneScript.HeadToToeAssementFinsihed && finishHeadtoToe.fontStyle != FontStyles.Strikethrough) { finishHeadtoToe.fontStyle = FontStyles.Strikethrough; }
        if (sceneScript.AppliedOxygen && AppliedOxygen.fontStyle != FontStyles.Strikethrough) { AppliedOxygen.fontStyle = FontStyles.Strikethrough; }
        if (sceneScript.AssessedIV && AssessIV.fontStyle != FontStyles.Strikethrough) { AssessIV.fontStyle = FontStyles.Strikethrough; }
        if (sceneScript.AnsweredFamilyQuestions && SpeakWithFamily.fontStyle != FontStyles.Strikethrough) { SpeakWithFamily.fontStyle = FontStyles.Strikethrough; }

        //checkpoint 3
        if (sceneScript.AssessedPain && AssessPain.fontStyle != FontStyles.Strikethrough) { AssessPain.fontStyle = FontStyles.Strikethrough; }
        if (sceneScript.AssessedWound && AssessWound.fontStyle != FontStyles.Strikethrough) { AssessWound.fontStyle = FontStyles.Strikethrough; }
        if (sceneScript.ObtainedWoundCulture && WoundCulture.fontStyle != FontStyles.Strikethrough) { WoundCulture.fontStyle = FontStyles.Strikethrough; }

        //checkpoint 4
        if (sceneScript.AdminsteredCAM && AdminsteredCam.fontStyle != FontStyles.Strikethrough) { AdminsteredCam.fontStyle = FontStyles.Strikethrough; }
        if (sceneScript.NotifiedPhysicianOfResults && NotifiedPhysician.fontStyle != FontStyles.Strikethrough) { NotifiedPhysician.fontStyle = FontStyles.Strikethrough; }

    }
}
