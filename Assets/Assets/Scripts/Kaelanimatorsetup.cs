using UnityEngine;
using UnityEditor;
using UnityEditor.Animations;
using System.Linq;
using System.Collections.Generic;

public class KaelAnimatorSetup : MonoBehaviour
{
#if UNITY_EDITOR
    [MenuItem("Tools/Setup Kael Animator")]
    static void CreateKaelAnimator()
    {
        string spritesheetPath = "Assets/personaje/Kael_Spritesheet.png";
        Sprite[] sprites = AssetDatabase.LoadAllAssetsAtPath(spritesheetPath)
            .OfType<Sprite>()
            .OrderBy(s => s.name)
            .ToArray();

        if (sprites.Length == 0)
        {
            Debug.LogError("No se encontraron sprites en: " + spritesheetPath);
            return;
        }

        Debug.Log($"Sprites encontrados: {sprites.Length}");

        var anims = new (string name, int start, int end, float fps, bool loop)[]
        {
            ("Kael_Idle",    0,  3,  8f,  true),
            ("Kael_Run",     4,  9, 10f,  true),
            ("Kael_Jump",   10, 13,  8f,  true),
            ("Kael_Blink",  14, 17, 12f, false),
            ("Kael_Attack", 18, 22, 12f, false),
            ("Kael_Hurt",   23, 24, 10f, false),
            ("Kael_Death",  25, 27,  8f, false),
        };

        string animFolder = "Assets/Animations";
        if (!AssetDatabase.IsValidFolder(animFolder))
            AssetDatabase.CreateFolder("Assets", "Animations");

        var clips = new Dictionary<string, AnimationClip>();

        foreach (var (name, start, end, fps, loop) in anims)
        {
            var clip = new AnimationClip();
            clip.frameRate = fps;

            var settings = AnimationUtility.GetAnimationClipSettings(clip);
            settings.loopTime = loop;
            AnimationUtility.SetAnimationClipSettings(clip, settings);

            int frameCount = end - start + 1;
            var keyframes = new ObjectReferenceKeyframe[frameCount];
            for (int i = 0; i < frameCount; i++)
            {
                int idx = start + i;
                if (idx >= sprites.Length) break;
                keyframes[i] = new ObjectReferenceKeyframe
                {
                    time = i / fps,
                    value = sprites[idx]
                };
            }

            var binding = new EditorCurveBinding
            {
                type = typeof(SpriteRenderer),
                path = "",
                propertyName = "m_Sprite"
            };

            AnimationUtility.SetObjectReferenceCurve(clip, binding, keyframes);

            string clipPath = $"{animFolder}/{name}.anim";
            AssetDatabase.CreateAsset(clip, clipPath);
            clips[name] = clip;
            Debug.Log($"Creada: {name} ({frameCount} frames a {fps} FPS)");
        }

        string ctrlPath = $"{animFolder}/KaelAnimator.controller";
        var ctrl = AnimatorController.CreateAnimatorControllerAtPath(ctrlPath);

        ctrl.AddParameter("Speed", AnimatorControllerParameterType.Float);
        ctrl.AddParameter("IsGrounded", AnimatorControllerParameterType.Bool);
        ctrl.AddParameter("VelocityY", AnimatorControllerParameterType.Float);
        ctrl.AddParameter("Throw", AnimatorControllerParameterType.Trigger);
        ctrl.AddParameter("Blink", AnimatorControllerParameterType.Trigger);
        ctrl.AddParameter("Attack", AnimatorControllerParameterType.Trigger);
        ctrl.AddParameter("Hit", AnimatorControllerParameterType.Trigger);
        ctrl.AddParameter("Die", AnimatorControllerParameterType.Trigger);

        var sm = ctrl.layers[0].stateMachine;

        var sIdle = sm.AddState("Idle");
        var sRun = sm.AddState("Run");
        var sJump = sm.AddState("Jump");
        var sBlink = sm.AddState("Blink");
        var sAtk = sm.AddState("Attack");
        var sHurt = sm.AddState("Hurt");
        var sDeath = sm.AddState("Death");

        sIdle.motion = clips["Kael_Idle"];
        sRun.motion = clips["Kael_Run"];
        sJump.motion = clips["Kael_Jump"];
        sBlink.motion = clips["Kael_Blink"];
        sAtk.motion = clips["Kael_Attack"];
        sHurt.motion = clips["Kael_Hurt"];
        sDeath.motion = clips["Kael_Death"];

        sm.defaultState = sIdle;

        // Idle <-> Run
        var t1 = sIdle.AddTransition(sRun);
        t1.hasExitTime = false; t1.duration = 0;
        t1.AddCondition(AnimatorConditionMode.Greater, 0.1f, "Speed");

        var t2 = sRun.AddTransition(sIdle);
        t2.hasExitTime = false; t2.duration = 0;
        t2.AddCondition(AnimatorConditionMode.Less, 0.1f, "Speed");

        // Idle / Run -> Jump
        var t3 = sIdle.AddTransition(sJump);
        t3.hasExitTime = false; t3.duration = 0;
        t3.AddCondition(AnimatorConditionMode.IfNot, 0, "IsGrounded");

        var t4 = sRun.AddTransition(sJump);
        t4.hasExitTime = false; t4.duration = 0;
        t4.AddCondition(AnimatorConditionMode.IfNot, 0, "IsGrounded");

        // Jump -> Idle
        var t5 = sJump.AddTransition(sIdle);
        t5.hasExitTime = false; t5.duration = 0;
        t5.AddCondition(AnimatorConditionMode.If, 0, "IsGrounded");

        // Any -> Blink
        var tb = sm.AddAnyStateTransition(sBlink);
        tb.hasExitTime = false; tb.duration = 0;
        tb.AddCondition(AnimatorConditionMode.If, 0, "Blink");

        // Any -> Attack
        var ta = sm.AddAnyStateTransition(sAtk);
        ta.hasExitTime = false; ta.duration = 0;
        ta.AddCondition(AnimatorConditionMode.If, 0, "Attack");

        // Any -> Hurt
        var th = sm.AddAnyStateTransition(sHurt);
        th.hasExitTime = false; th.duration = 0;
        th.AddCondition(AnimatorConditionMode.If, 0, "Hit");

        // Any -> Death
        var td = sm.AddAnyStateTransition(sDeath);
        td.hasExitTime = false; td.duration = 0;
        td.AddCondition(AnimatorConditionMode.If, 0, "Die");

        // Blink / Attack / Hurt -> Idle
        var rb2 = sBlink.AddTransition(sIdle);
        rb2.hasExitTime = true; rb2.exitTime = 1f; rb2.duration = 0;

        var ra = sAtk.AddTransition(sIdle);
        ra.hasExitTime = true; ra.exitTime = 1f; ra.duration = 0;

        var rh = sHurt.AddTransition(sIdle);
        rh.hasExitTime = true; rh.exitTime = 1f; rh.duration = 0;

        EditorUtility.SetDirty(ctrl);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Debug.Log("LISTO. Ve a Assets/Animations/ y arrastra KaelAnimator al Animator de Kael.");
    }
#endif
}

