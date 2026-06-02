using UnityEngine;
using UnityEditor;
using UnityEditor.Animations;
using System.Linq;
using System.Collections.Generic;

public class FalangeAnimatorSetup : MonoBehaviour
{
#if UNITY_EDITOR
    [MenuItem("Tools/Setup Falange Animator")]
    static void CreateFalangeAnimator()
    {
        // ── RUTA DEL SPRITESHEET ──
        string path = "Assets/Assets/personje/FalangeExecutor_Spritesheet_Transparent.png"; ;

        // Intentar ruta alternativa si no encuentra
        Sprite[] sprites = AssetDatabase.LoadAllAssetsAtPath(path)
            .OfType<Sprite>()
            .OrderBy(s => s.name)
            .ToArray();

        if (sprites.Length == 0)
        {
            // Intentar con guion bajo
            path = "Assets/Assets/personaje/FalangeExecutor_Spritesheet.png";
            sprites = AssetDatabase.LoadAllAssetsAtPath(path)
                .OfType<Sprite>()
                .OrderBy(s => s.name)
                .ToArray();
        }

        if (sprites.Length == 0)
        {
            Debug.LogError("No se encontraron sprites. Verifica que el spritesheet esté en Assets/Assets/personaje/ y que tenga Sprite Mode: Multiple con el slice aplicado.");
            return;
        }

        Debug.Log($"Sprites encontrados: {sprites.Length}");

        // Fila, nombre, fps, loop
        var anims = new (int row, string name, float fps, bool loop)[]
        {
            (0, "Falange_Idle",         8f, true),
            (1, "Falange_Walk",        10f, true),
            (2, "Falange_Alert",       10f, false),
            (3, "Falange_Attack",      12f, false),
            (4, "Falange_Block",       10f, false),
            (5, "Falange_ShieldBreak", 10f, false),
            (6, "Falange_Hurt",        10f, false),
            (7, "Falange_Death",        8f, false),
        };

        // Crear carpeta si no existe
        if (!AssetDatabase.IsValidFolder("Assets/Animation"))
            AssetDatabase.CreateFolder("Assets", "Animation");
        if (!AssetDatabase.IsValidFolder("Assets/Animation/Enemies"))
            AssetDatabase.CreateFolder("Assets/Animation", "Enemies");

        string folder = "Assets/Animation/Enemies";
        var clips = new Dictionary<string, AnimationClip>();

        foreach (var (row, name, fps, loop) in anims)
        {
            var clip = new AnimationClip { frameRate = fps };
            var settings = AnimationUtility.GetAnimationClipSettings(clip);
            settings.loopTime = loop;
            AnimationUtility.SetAnimationClipSettings(clip, settings);

            // 8 frames por fila
            int frameCount = 8;
            var keys = new ObjectReferenceKeyframe[frameCount];
            for (int i = 0; i < frameCount; i++)
            {
                int idx = row * frameCount + i;
                keys[i] = new ObjectReferenceKeyframe
                {
                    time  = i / fps,
                    value = idx < sprites.Length ? sprites[idx] : sprites[sprites.Length - 1]
                };
            }

            var binding = new EditorCurveBinding
            {
                type         = typeof(SpriteRenderer),
                path         = "",
                propertyName = "m_Sprite"
            };
            AnimationUtility.SetObjectReferenceCurve(clip, binding, keys);

            string clipPath = $"{folder}/{name}.anim";
            // Borrar si ya existe
            AssetDatabase.DeleteAsset(clipPath);
            AssetDatabase.CreateAsset(clip, clipPath);
            clips[name] = clip;
            Debug.Log($"Clip creado: {name}");
        }

        // ── ANIMATOR CONTROLLER ──
        string ctrlPath = $"{folder}/FalangeAnimator.controller";
        AssetDatabase.DeleteAsset(ctrlPath);
        var ctrl = AnimatorController.CreateAnimatorControllerAtPath(ctrlPath);

        ctrl.AddParameter("Speed",        AnimatorControllerParameterType.Float);
        ctrl.AddParameter("Alert",        AnimatorControllerParameterType.Bool);
        ctrl.AddParameter("ShieldActive", AnimatorControllerParameterType.Bool);
        ctrl.AddParameter("Attack",       AnimatorControllerParameterType.Trigger);
        ctrl.AddParameter("Block",        AnimatorControllerParameterType.Trigger);
        ctrl.AddParameter("Hit",          AnimatorControllerParameterType.Trigger);
        ctrl.AddParameter("Die",          AnimatorControllerParameterType.Trigger);

        var sm = ctrl.layers[0].stateMachine;

        var sIdle  = sm.AddState("Idle");        sIdle.motion  = clips["Falange_Idle"];
        var sWalk  = sm.AddState("Walk");        sWalk.motion  = clips["Falange_Walk"];
        var sAlert = sm.AddState("Alert");       sAlert.motion = clips["Falange_Alert"];
        var sAtk   = sm.AddState("Attack");      sAtk.motion   = clips["Falange_Attack"];
        var sBlock = sm.AddState("Block");       sBlock.motion = clips["Falange_Block"];
        var sSB    = sm.AddState("ShieldBreak"); sSB.motion    = clips["Falange_ShieldBreak"];
        var sHurt  = sm.AddState("Hurt");        sHurt.motion  = clips["Falange_Hurt"];
        var sDie   = sm.AddState("Death");       sDie.motion   = clips["Falange_Death"];

        sm.defaultState = sIdle;

        // Helper transición normal
        void AddTransition(AnimatorState from, AnimatorState to, bool hasExit,
                           string param = null,
                           AnimatorConditionMode mode = AnimatorConditionMode.If,
                           float val = 0)
        {
            var tr = from.AddTransition(to);
            tr.hasExitTime = hasExit;
            tr.exitTime    = 1f;
            tr.duration    = 0f;
            if (param != null) tr.AddCondition(mode, val, param);
        }

        // Helper AnyState
        void AddAnyTransition(AnimatorState to, string trigger)
        {
            var tr = sm.AddAnyStateTransition(to);
            tr.hasExitTime = false;
            tr.duration    = 0f;
            tr.AddCondition(AnimatorConditionMode.If, 0, trigger);
        }

        // Idle <-> Walk
        AddTransition(sIdle, sWalk,  false, "Speed", AnimatorConditionMode.Greater, 0.1f);
        AddTransition(sWalk, sIdle,  false, "Speed", AnimatorConditionMode.Less,    0.1f);

        // Idle/Walk <-> Alert
        AddTransition(sIdle,  sAlert, false, "Alert", AnimatorConditionMode.If);
        AddTransition(sWalk,  sAlert, false, "Alert", AnimatorConditionMode.If);
        AddTransition(sAlert, sIdle,  false, "Alert", AnimatorConditionMode.IfNot);

        // AnyState triggers
        AddAnyTransition(sAtk,   "Attack");
        AddAnyTransition(sBlock, "Block");
        AddAnyTransition(sHurt,  "Hit");
        AddAnyTransition(sDie,   "Die");

        // Volver a Alert al terminar
        AddTransition(sAtk,   sAlert, true);
        AddTransition(sBlock, sAlert, true);
        AddTransition(sHurt,  sAlert, true);
        AddTransition(sSB,    sHurt,  true);

        EditorUtility.SetDirty(ctrl);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Debug.Log("LISTO — FalangeAnimator.controller creado en Assets/Animation/Enemies/");
    }
#endif
}
