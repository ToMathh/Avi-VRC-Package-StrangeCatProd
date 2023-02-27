#if UNITY_EDITOR

// This wouldn't have been posible without the works of VRLabs. There is no way in the world I would have figured this out on my own without a reference
// Special thanks to ksivl for writing script functions and giving it a permissive license ♥

using System.IO;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine.Animations;
using UnityEngine;
using VRC.SDK3.Avatars.Components;
using VRC.SDK3.Avatars.ScriptableObjects;
using Vector3 = UnityEngine.Vector3;

using UnityEditor.AnimatedValues;
using UnityEngine.Events;


namespace GabSith.AFKMuteBadge
{

    [CustomEditor(typeof(Installer))]
    public class AFKMuteBadgeEditor : Editor
    {
        public VRCAvatarDescriptor descriptor;
        public Animator avatar;
        //float baseSize = 100f;
        bool goodToGo = true;
        bool useWriteDefaults = false;

        string[] platformStrings = { "PC", "Quest" };

        bool fold = false;
        AnimBool drop;

        //Transform badge;

        SerializedObject so;
        SerializedProperty _size;
        SerializedProperty _position;

        Vector3 posSize = Vector3.one;
        Vector3 sizeSize = Vector3.one;

        Vector3 posHandle;

        private void OnEnable()
        {
            so = serializedObject;
            _size = so.FindProperty("size");
            _position = so.FindProperty("position");


            drop = new AnimBool(fold);
            drop.valueChanged.AddListener(new UnityAction(base.Repaint));


            if (((Installer)target).gameObject.GetComponent<VRCAvatarDescriptor>() != null)
                descriptor = ((Installer)target).gameObject.GetComponent<VRCAvatarDescriptor>();

            if (ScriptFunctions.HasMixedWriteDefaults(descriptor) == ScriptFunctions.WriteDefaults.Off)
            {
                useWriteDefaults = false;
            }
            else
            {
                useWriteDefaults = true;
            }

        }


        void OnSceneGUI()
        {
            if (((Installer)target).badgeSpawned && ((Installer)target).platform == 0)
            {
                if (((Installer)target).badge == null)
                {
                    ((Installer)target).badge = descriptor.transform.Find("AFK Mute Badge").Find("Container").Find("Badge");
                }

                posHandle = Handles.PositionHandle(((Installer)target).badge.position, ((Installer)target).badge.rotation);
                ((Installer)target).badge.position = posHandle;
            }
        }

        public override void OnInspectorGUI()
        {

            GUIStyle boxStyle = new GUIStyle("box") { stretchWidth = true };
            boxStyle.normal.textColor = new GUIStyle("label").normal.textColor;
            GUIStyle titleStyle = new GUIStyle(GUI.skin.label) { alignment = TextAnchor.MiddleCenter, richText = true };

            GUIStyle discordSpam = new GUIStyle(EditorStyles.label)
            {
                fontStyle = FontStyle.Bold,
                hover = { textColor = Color.red },
                normal = { textColor = Color.grey },
                alignment = TextAnchor.MiddleRight,
                margin = { right = 10, bottom = 5 },
                fontSize = 11
            };
            GUIStyle toolPlat = new GUIStyle(EditorStyles.miniButtonMid) { fixedHeight = 35, fontStyle = FontStyle.Bold, fontSize = 13 };

            GUIStyle buttonLeft = new GUIStyle(EditorStyles.miniButton) {  };


            GUILayout.Space(8);
            EditorGUILayout.BeginHorizontal(boxStyle);
            EditorGUILayout.LabelField("<b><size=14>AFK-Mute Badge</size></b> <size=9> by GabSith</size>", titleStyle, GUILayout.MinHeight(50f));
            EditorGUILayout.EndHorizontal();
            GUILayout.Space(15);
            //<size=18>\n</size>


            if (!AssetDatabase.IsValidFolder("Assets/AFK-Mute Badge/Assets"))
            {
                EditorGUILayout.HelpBox("'AFK-Mute Badge' directory was not found! Make sure you don't move or delete the folder containing the badge's assets before installing!", MessageType.Error, true);
                EditorGUILayout.HelpBox("If you want better organization you can move the assets after setting the badge up", MessageType.Info, true);
                GUILayout.Space(30);
                return;
            }



            descriptor = (VRCAvatarDescriptor)EditorGUILayout.ObjectField("Avatar", descriptor, typeof(VRCAvatarDescriptor), true);
            if (descriptor != null)
            {
                avatar = descriptor.gameObject.GetComponent<Animator>();
            }

            GUIStyle buttonStyle = new GUIStyle(GUI.skin.button) { fontStyle = FontStyle.Bold, fontSize = 15 };


            GUILayout.Space(20);

            if (goodToGo || (((Installer)target).badgeSpawned))
            {

                if (!((Installer)target).badgeSpawned)
                {
                    CheckRequirements();
                    GUILayout.Space(20);
                }


                if (!((Installer)target).done)
                {

                    if (((Installer)target).badgeSpawned)
                    {
                        if (GUILayout.Button(new GUIContent("Finish Setup", "Removes the script and hides the badge, so the avatar is ready to upload"), buttonStyle, GUILayout.Height(40)))
                        {
                            ((Installer)target).done = true;
                        }
                    }
                    else
                    {
                        // Platform
                        //((Installer)target).platform = GUILayout.Toolbar(((Installer)target).platform, platformStrings, toolPlat);
                        ((Installer)target).platform = EditorGUILayout.Popup(new GUIContent("Platform"), ((Installer)target).platform, platformStrings);

                        // platform == 0 -> PC
                        // platform == 1 -> Quest

                        GUILayout.Space(20);

                        if (GUILayout.Button(new GUIContent("Set Up Badge", "Adds the necesary FX Layers, Parameters, GameObjects and Menus automatically"), buttonStyle, GUILayout.Height(50)))
                        {
                            if (((Installer)target).platform == 0)
                            {
                                BadgeSetUp();
                                Spawn();
                                ((Installer)target).badgeSpawned = true;
                            }
                            else
                            {
                                BadgeSetUpQuest();
                                SpawnQuest();
                                ((Installer)target).badgeSpawned = true;
                            }
                        }
                    }

                    GUILayout.Space(10);



                    // Change Size

                    if (((Installer)target).badgeSpawned)
                    {
                        /*
                        so = serializedObject;
                        _size = so.FindProperty("size");
                        _position = so.FindProperty("position");*/


                            so = serializedObject;
                            _size = so.FindProperty("size");
                            _position = so.FindProperty("position");

                            so.Update();

                        EditorGUILayout.PropertyField(_size);
                        if (so.ApplyModifiedProperties())
                        {

                            if (((Installer)target).platform == 0)
                            {
                                ChangeSize();
                            }
                            else
                            {
                                ChangeSizeQuest();
                            }
                        }
                    }


                    // Position Transform
                    if (((Installer)target).badgeSpawned)
                    {
                        if (((Installer)target).platform == 0)
                        {
                            /*
                            posTransform = GUILayout.Button("hola", buttonLeft);
                            if (posTransform) {
                                Debug.LogError("aa");
                            }*/

                            //posGizmo = EditorGUILayout.ToggleLeft("Position Gizmo", posGizmo);

                            /*
                            Color highlightColor = new Color(1f, 0.35f, 0.35f);
                            //GUI.backgroundColor = posTransform ? Color.white : highlightColor;
                            if (!posTransform)
                                GUI.backgroundColor = highlightColor;
                            else
                                GUI.backgroundColor = Color.white;
                            if (GUILayout.Button("Move")) posTransform = false;
                            if (posTransform)
                                GUI.backgroundColor = highlightColor;
                            else
                                GUI.backgroundColor = Color.white;
                            */
                            
                            EditorGUILayout.Separator();
                            if (GUILayout.Button(new GUIContent("Change Position", "Move the badge to it's middle point")))
                            {
                                Transform parentBadge = descriptor.transform.Find("AFK Mute Badge");
                                Transform cont = parentBadge.transform.Find("Container");
                                ((Installer)target).badge = cont.Find("Badge");

                                if (((Installer)target).badge.gameObject == null)
                                {
                                    Debug.LogError("Can't find badge! It may have been moved or deleted.");
                                }
                                else
                                {
                                    //Selection.activeGameObject = badge.gameObject;
                                    //Selection.SetActiveObjectWithContext(badge.gameObject, (Installer)target);
                                    Selection.activeTransform = ((Installer)target).badge;

                                }
                            }
                        }
                        else
                        {
                            QuestPosition();
                        }
                    }

                }
                else
                {
                    //GUILayout.Space(20);
                    GUILayout.Label("This will remove the script and hide the badge");
                    /*
                    if (GUILayout.Button(new GUIContent("Apply", "This will remove the script and hide the badge. You'll have to run the installer again to modify things easily"), buttonStyle, GUILayout.Height(50)))
                    {
                        ((Installer)target).done = true;
                        AssetDatabase.SaveAssets();
                        AssetDatabase.Refresh();
                        Debug.Log("Successfully generated!");

                        Transform parentBadge = descriptor.transform.Find("AFK Mute Badge");
                        Transform cont = parentBadge.transform.Find("Container");
                        Transform badge = cont.Find("Badge");

                        Transform bord = badge.Find("Border");
                        Transform cent = badge.Find("Center");

                        bord.gameObject.SetActive(false);
                        cent.gameObject.SetActive(false);
                        ((Installer)target).DestroyScriptInstance();
                        ((Installer)target).apply = true;
                    }*/

                    using (new EditorGUILayout.HorizontalScope())
                    {

                        if (GUILayout.Button("Resume Setup", GUILayout.Height(40)))
                        {
                            ((Installer)target).done = false;
                        }
                        if (GUILayout.Button("Apply", buttonStyle, GUILayout.Height(40)))
                        {

                            if (((Installer)target).platform == 0)
                            {
                                Transform parentBadge = descriptor.transform.Find("AFK Mute Badge");
                                Transform cont = parentBadge.transform.Find("Container");
                                Transform badge = cont.Find("Badge");

                                Transform bord = badge.Find("Border");
                                Transform cent = badge.Find("Center");

                                bord.gameObject.SetActive(false);
                                cent.gameObject.SetActive(false);
                            }
                            else
                            {
                                Transform badge = descriptor.transform.Find("Badge");
                                Transform qbadge = badge.transform.Find("Quest Badge");

                                qbadge.gameObject.SetActive(false);
                            }



                            ((Installer)target).done = true;

                            ((Installer)target).DestroyScriptInstance();
                            ((Installer)target).apply = true;
                        }
                    }

                    /*
                    if (GUILayout.Button("Resume Setup"))
                    {
                        ((Installer)target).done = false;
                    }*/
                }

            }

            else if (!((Installer)target).badgeSpawned)
            {
                //GUILayout.Label("Requirements to add the badge are not met, check the console for more info");

                // GUILayout.Toolbar(2)


                CheckRequirements();

            }

            GUILayout.Space(10);


            drop.target = fold;

            EditorGUI.indentLevel++;
            fold = EditorGUILayout.Foldout(fold, " Extra");
            EditorGUI.indentLevel--;

            using (var group = new EditorGUILayout.FadeGroupScope(drop.faded))
            {
                if (group.visible)
                {
                    if (GUILayout.Button(new GUIContent("Remove Components", "Removes all related GameObjects, Parameters, FX layers")))
                    {

                        if (EditorUtility.DisplayDialog("Remove Badge", "Remove AFK-Mute Badge from the avatar? (this will also remove all related Parameters, FX layers, etc.)", "Yes", "No"))
                        {
                            Remove();
                            ((Installer)target).badgeSpawned = false;
                            Debug.Log("Parameters Removed");
                        }
                    }
                    if (GUILayout.Button(new GUIContent("Remove Script", "Removes this script from the avatar")))
                    {
                        ((Installer)target).DestroyScriptInstance();
                    }
                    if (!((Installer)target).badgeSpawned)
                        useWriteDefaults = EditorGUILayout.ToggleLeft(new GUIContent("Use Write Defaults"), useWriteDefaults);

                    //posGizmo = EditorGUILayout.ToggleLeft("Position Gizmo", posGizmo);
                    if (((Installer)target).badgeSpawned && ((Installer)target).platform == 0)
                        EditorGUILayout.HelpBox("Tip: you can change the badge´s position by using the gizmo in the scene", MessageType.Info);

                }
            }

            GUILayout.Space(5);

            if (GUILayout.Button(new GUIContent("Discord", "Problems? Feedback? Here's my discord server"), discordSpam))
            {
                Application.OpenURL("https://discord.gg/uvYW2N4eW9");
            }

            GUILayout.Space(5);
        }


        void BadgeSetUp()
        {
            Remove();

            Directory.CreateDirectory("Assets/AFK-Mute Badge/Assets/Generated Assets/");
            AssetDatabase.Refresh();

            string directory = "Assets/AFK-Mute Badge/Assets/Generated Assets/";

            // Parameters
            VRCExpressionParameters.Parameter
                b_AFKMute = new VRCExpressionParameters.Parameter
                { name = "AFKMuteBadge_AFK Mute", valueType = VRCExpressionParameters.ValueType.Bool, saved = true, defaultValue = 1 },
                b_AFKSync = new VRCExpressionParameters.Parameter
                { name = "AFKMuteBadge_AFKSync", valueType = VRCExpressionParameters.ValueType.Bool, saved = false },
                b_MuteSync = new VRCExpressionParameters.Parameter
                { name = "AFKMuteBadge_MuteSync", valueType = VRCExpressionParameters.ValueType.Bool, saved = false },
                b_AutoAFK = new VRCExpressionParameters.Parameter
                { name = "AFKMuteBadge_Auto AFK", valueType = VRCExpressionParameters.ValueType.Bool, saved = true },
                b_AutoMute = new VRCExpressionParameters.Parameter
                { name = "AFKMuteBadge_Auto Mute", valueType = VRCExpressionParameters.ValueType.Bool, saved = true },
                b_AFKMuteOn = new VRCExpressionParameters.Parameter
                { name = "AFKMuteBadge_AFK Mute On", valueType = VRCExpressionParameters.ValueType.Bool, saved = true };

            ScriptFunctions.AddParameter(descriptor, b_AFKMute, directory);
            ScriptFunctions.AddParameter(descriptor, b_AFKSync, directory);
            ScriptFunctions.AddParameter(descriptor, b_MuteSync, directory);
            ScriptFunctions.AddParameter(descriptor, b_AutoAFK, directory);
            ScriptFunctions.AddParameter(descriptor, b_AutoMute, directory);
            ScriptFunctions.AddParameter(descriptor, b_AFKMuteOn, directory);

            Debug.Log("Parameters Added");

            // FX

            if (!useWriteDefaults)
            {
                AssetDatabase.CopyAsset("Assets/AFK-Mute Badge/Assets/FX.controller", directory + "FXtemp.controller");
                AnimatorController FX = AssetDatabase.LoadAssetAtPath(directory + "FXtemp.controller", typeof(AnimatorController)) as AnimatorController;
                Debug.Log("FX Added");


                ScriptFunctions.MergeController(descriptor, FX, ScriptFunctions.PlayableLayer.FX, directory);
                AssetDatabase.DeleteAsset(AssetDatabase.GetAssetPath(FX)); // delete temporary FX layer
                Debug.Log("FX Merged");
            }
            else
            {
                AssetDatabase.CopyAsset("Assets/AFK-Mute Badge/Assets/FX w WD.controller", directory + "FXtemp.controller");
                AnimatorController FX = AssetDatabase.LoadAssetAtPath(directory + "FXtemp.controller", typeof(AnimatorController)) as AnimatorController;
                Debug.Log("FX Added");


                ScriptFunctions.MergeController(descriptor, FX, ScriptFunctions.PlayableLayer.FX, directory);
                AssetDatabase.DeleteAsset(AssetDatabase.GetAssetPath(FX)); // delete temporary FX layer
                Debug.Log("FX Merged");
            }

            // Menu
            AssetDatabase.CopyAsset("Assets/AFK-Mute Badge/Assets/Badge Menu.asset", directory + "Badge Menu.asset");
            VRCExpressionsMenu badgeMenu = AssetDatabase.LoadAssetAtPath(directory + "Badge Menu.asset", typeof(VRCExpressionsMenu)) as VRCExpressionsMenu;

            VRCExpressionsMenu.Control.Parameter par_menu = new VRCExpressionsMenu.Control.Parameter
            { name = "" };
            Texture2D badgeIcon = AssetDatabase.LoadAssetAtPath("Assets/AFK-Mute Badge/Assets/Icons/Auto Mute AFK.png", typeof(Texture2D)) as Texture2D;
            ScriptFunctions.AddSubMenu(descriptor, badgeMenu, "AFK-Mute Badge", directory, par_menu, badgeIcon);
            Debug.Log("Menu Added");


        }

        void Spawn()
        {
            Transform head = avatar.GetBoneTransform(HumanBodyBones.Head);
            GameObject badgeGO = PrefabUtility.InstantiatePrefab(AssetDatabase.LoadAssetAtPath("Assets/AFK-Mute Badge/Assets/AFK Mute Badge.prefab", typeof(GameObject)), avatar.transform) as GameObject;


            if (PrefabUtility.IsPartOfPrefabInstance(badgeGO))
                PrefabUtility.UnpackPrefabInstance(badgeGO, PrefabUnpackMode.Completely, InteractionMode.AutomatedAction);
            //badge.transform.SetParent(avatar.transform, false);

            ((Installer)target).badgeST = badgeGO.transform.Find("Badge_SpringTarget");
            ((Installer)target).badgeST.transform.SetParent(head.transform, false);

            ((Installer)target).badgeST.localPosition = new Vector3(0f, 0f, 0f);

            ((Installer)target).badge = badgeGO.transform.Find("AFK Mute Badge");

            //GetHeight();
            
            //((Installer)target).badge = badge.transform.Find("AFK Mute Badge");
            //Transform container = badge.transform.Find("Container");
            //Transform badgeT = container.Find("Badge");
            

            Transform parentBadge = descriptor.transform.Find("AFK Mute Badge");
            Transform cont = parentBadge.transform.Find("Container");
            ((Installer)target).badge = cont.Find("Badge");


            if (((Installer)target).badge == null)
                Debug.LogError("badgeT is null");

            badgeGO.transform.position = new Vector3(0, head.position.y, 0);

            var badgeConst = ((Installer)target).badge.GetComponent("RotationConstraint");
            ConstraintSource badgePar = new ConstraintSource { sourceTransform = descriptor.transform, weight = 1f };
            badgeConst.GetComponent<RotationConstraint>().SetSource(0, badgePar);

        }

        void ChangeSize()
        {
            /*Transform parentBadge = descriptor.transform.Find("AFK Mute Badge");
            Transform cont = parentBadge.transform.Find("Container");
            Transform badge = cont.Find("Badge");
            */
            if (((Installer)target).badge == null)
            {
                Transform parentBadge = descriptor.transform.Find("AFK Mute Badge");
                Transform cont = parentBadge.transform.Find("Container");
                ((Installer)target).badge = cont.Find("Badge");
            }
            Transform bord = ((Installer)target).badge.Find("Border");
            Transform cent = ((Installer)target).badge.Find("Center");

            bord.localScale = Vector3.one * _size.floatValue * 100;
            cent.localScale = Vector3.one * _size.floatValue * 100;

        }

        void BadgeSetUpQuest()
        {
            Remove();

            Directory.CreateDirectory("Assets/AFK-Mute Badge/Assets/Quest/Quest Assets/Generated Assets/");
            AssetDatabase.Refresh();

            string directory = "Assets/AFK-Mute Badge/Assets/Quest/Quest Assets/Generated Assets/";

            // Parameters
            VRCExpressionParameters.Parameter
                b_AFKMute = new VRCExpressionParameters.Parameter
                { name = "AFKMuteBadge_AFK Mute", valueType = VRCExpressionParameters.ValueType.Bool, saved = true, defaultValue = 1 },
                b_AFKSync = new VRCExpressionParameters.Parameter
                { name = "AFKMuteBadge_AFKSync", valueType = VRCExpressionParameters.ValueType.Bool, saved = false },
                b_MuteSync = new VRCExpressionParameters.Parameter
                { name = "AFKMuteBadge_MuteSync", valueType = VRCExpressionParameters.ValueType.Bool, saved = false },
                b_AutoAFK = new VRCExpressionParameters.Parameter
                { name = "AFKMuteBadge_Auto AFK", valueType = VRCExpressionParameters.ValueType.Bool, saved = true },
                b_AutoMute = new VRCExpressionParameters.Parameter
                { name = "AFKMuteBadge_Auto Mute", valueType = VRCExpressionParameters.ValueType.Bool, saved = true },
                b_AFKMuteOn = new VRCExpressionParameters.Parameter
                { name = "AFKMuteBadge_AFK Mute On", valueType = VRCExpressionParameters.ValueType.Bool, saved = true };

            ScriptFunctions.AddParameter(descriptor, b_AFKMute, directory);
            ScriptFunctions.AddParameter(descriptor, b_AFKSync, directory);
            ScriptFunctions.AddParameter(descriptor, b_MuteSync, directory);
            ScriptFunctions.AddParameter(descriptor, b_AutoAFK, directory);
            ScriptFunctions.AddParameter(descriptor, b_AutoMute, directory);
            ScriptFunctions.AddParameter(descriptor, b_AFKMuteOn, directory);

            Debug.Log("Parameters Added");

            // FX
            if (!useWriteDefaults)
            {
                AssetDatabase.CopyAsset("Assets/AFK-Mute Badge/Assets/Quest/Quest Assets/FX Quest.controller", directory + "FXtemp.controller");
                AnimatorController FX = AssetDatabase.LoadAssetAtPath(directory + "FXtemp.controller", typeof(AnimatorController)) as AnimatorController;
                Debug.Log("FX Added");


                ScriptFunctions.MergeController(descriptor, FX, ScriptFunctions.PlayableLayer.FX, directory);
                AssetDatabase.DeleteAsset(AssetDatabase.GetAssetPath(FX)); // delete temporary FX layer
                Debug.Log("FX Merged");
            }
            else
            {
                AssetDatabase.CopyAsset("Assets/AFK-Mute Badge/Assets/Quest/Quest Assets/FX Quest w WD.controller", directory + "FXtemp.controller");
                AnimatorController FX = AssetDatabase.LoadAssetAtPath(directory + "FXtemp.controller", typeof(AnimatorController)) as AnimatorController;
                Debug.Log("FX Added");


                ScriptFunctions.MergeController(descriptor, FX, ScriptFunctions.PlayableLayer.FX, directory);
                AssetDatabase.DeleteAsset(AssetDatabase.GetAssetPath(FX)); // delete temporary FX layer
                Debug.Log("FX Merged");
            }
            // Menu
            AssetDatabase.CopyAsset("Assets/AFK-Mute Badge/Assets/Badge Menu.asset", directory + "Badge Menu.asset");
            VRCExpressionsMenu badgeMenu = AssetDatabase.LoadAssetAtPath(directory + "Badge Menu.asset", typeof(VRCExpressionsMenu)) as VRCExpressionsMenu;

            VRCExpressionsMenu.Control.Parameter par_menu = new VRCExpressionsMenu.Control.Parameter
            { name = "" };
            Texture2D badgeIcon = AssetDatabase.LoadAssetAtPath("Assets/AFK-Mute Badge/Assets/Icons/Auto Mute AFK.png", typeof(Texture2D)) as Texture2D;
            ScriptFunctions.AddSubMenu(descriptor, badgeMenu, "AFK-Mute Badge", directory, par_menu, badgeIcon);
            Debug.Log("Menu Added");
        }

        void SpawnQuest()
        {
            //Transform head = avatar.GetBoneTransform(HumanBodyBones.Head);
            GameObject badge = PrefabUtility.InstantiatePrefab(AssetDatabase.LoadAssetAtPath("Assets/AFK-Mute Badge/Assets/Quest/Badge.prefab", typeof(GameObject)), avatar.transform) as GameObject;

            if (PrefabUtility.IsPartOfPrefabInstance(badge))
                PrefabUtility.UnpackPrefabInstance(badge, PrefabUnpackMode.Completely, InteractionMode.AutomatedAction);
            //badge.transform.SetParent(avatar.transform, false);


        }

        void ChangeSizeQuest()
        {
            Transform parentBadge = descriptor.transform.Find("Badge");
            Transform QuestBadge = parentBadge.transform.Find("Quest Badge");
            Transform Core = QuestBadge.transform.Find("Core");
            //Core.localScale = new Vector3(posSize.x + _size.floatValue, posSize.y + _size.floatValue, posSize.z + _size.floatValue);
            Core.localScale = Vector3.one * _size.floatValue;
            sizeSize = Core.localScale;
            Core.localScale /= _position.floatValue;
        }

        void QuestPosition()
        {
            so.Update();
            EditorGUILayout.PropertyField(_position);
            if (so.ApplyModifiedProperties())
            {
                Transform parentBadge = descriptor.transform.Find("Badge");
                Transform QuestBadge = parentBadge.transform.Find("Quest Badge");
                Transform Core = QuestBadge.transform.Find("Core");

                parentBadge.localScale = Vector3.one * _position.floatValue;
                Core.localScale = sizeSize / _position.floatValue;
                posSize = Core.localScale;

                //Core.localScale = Core.localScale / _position.floatValue;
            }
        }

        void Remove()
        {
            ScriptFunctions.UninstallControllerByPrefix(descriptor, "AFKMuteBadge_", ScriptFunctions.PlayableLayer.FX);
            ScriptFunctions.UninstallParametersByPrefix(descriptor, "AFKMuteBadge_");
            ScriptFunctions.UninstallMenu(descriptor, "AFK-Mute Badge");

            Transform AFKMuteBadge = descriptor.transform.Find("AFK Mute Badge");
            if (AFKMuteBadge != null)
                DestroyImmediate(AFKMuteBadge.gameObject);

            Transform AFKMuteBadgeQuest = descriptor.transform.Find("Badge");
            if (AFKMuteBadgeQuest != null)
                DestroyImmediate(AFKMuteBadgeQuest.gameObject);

            Transform head = avatar.GetBoneTransform(HumanBodyBones.Head);
            Transform AFKMuteSP = head.transform.Find("Badge_SpringTarget");
            if (AFKMuteSP != null)
                DestroyImmediate(AFKMuteSP.gameObject);
        }

        private void CheckRequirements()
        {
            goodToGo = true;

            if (descriptor == null)
            {
                //Debug.LogError("There is no avatar descriptor on this GameObject. Please move this script onto your avatar, or create an avatar descriptor here.");
                EditorGUILayout.HelpBox("There is no avatar descriptor on this GameObject. Please move this script onto your avatar, or create an avatar descriptor here.", MessageType.Error, true);
                goodToGo = false;
            }
            else
            {
                if (descriptor.expressionParameters != null && descriptor.expressionParameters.CalcTotalCost() > (256 - 6))
                {
                    //Debug.LogError("You don't have enough free memory in your avatar's Expression Parameters to generate. You need at least 1 bits of parameter memory available.");
                    EditorGUILayout.HelpBox("You don't have enough free memory in your avatar's Expression Parameters to generate. You need at least 6 bits of parameter memory available.", MessageType.Error, true);

                    goodToGo = false;
                }
                if (descriptor.expressionsMenu != null)
                {
                    if (descriptor.expressionsMenu.controls.Count == 8)
                    {
                        //Debug.LogError("Your avatar's topmost menu is full. Please have at least one empty control slot available.");
                        EditorGUILayout.HelpBox("Your avatar's topmost menu is full. Please have at least one empty control slot available.", MessageType.Error, true);

                        goodToGo = false;
                    }
                }
            }

            if (PrefabUtility.IsPartOfPrefabInstance(descriptor.transform))
            {
                //Debug.LogError("The avatar is a prefab. Unpack it to be able to edit it.");
                EditorGUILayout.HelpBox("The avatar is a prefab. Unpack it to be able to edit it.", MessageType.Error, true);
                goodToGo = false;
                // PrefabUtility.UnpackPrefabInstance(descriptor.transform, PrefabUnpackMode.Completely, InteractionMode.AutomatedAction);
            }


            if (avatar == null)
            {
                //Debug.LogError("There is no Animator on this avatar. Please add an Animator component on your avatar.");
                EditorGUILayout.HelpBox("There is no Animator on this avatar. Please add an Animator component on your avatar.", MessageType.Error, true);

                goodToGo = false;
            }
            else
            {
                if (!avatar.isHuman)
                {
                    //Debug.LogError("Please use this script on an avatar with a humanoid rig.");
                    EditorGUILayout.HelpBox("Please use this script on an avatar with a humanoid rig.", MessageType.Error, true);

                    goodToGo = false;
                }
                if (avatar.GetBoneTransform(HumanBodyBones.Head) == null)
                {
                    //Debug.LogError("Your avatar rig's head is unmapped!");
                    EditorGUILayout.HelpBox("Your avatar rig's head is unmapped!", MessageType.Error, true);

                    goodToGo = false;
                }

            }

            var wd = ScriptFunctions.HasMixedWriteDefaults(descriptor);

            if (wd == ScriptFunctions.WriteDefaults.On)
            {
                EditorGUILayout.HelpBox("This avatar uses write defaults set to on, which is not recommended by VRChat. " +
                    "The animations' settings will change to avoid problems, but this is not supported.", MessageType.Warning, true);
                EditorGUILayout.HelpBox("You can change the Write Defaults mode in 'Extra'", MessageType.Info, true);

                ((Installer)target).writeDefaultEnabled = true;
            }
            else if (wd == ScriptFunctions.WriteDefaults.Mixed)
            {
                EditorGUILayout.HelpBox("This avatar uses mixed write defaults settings. " +
                    "This can (and most likely will) cause some strange behaviors, such as facial animations getting stuck. " +
                    "\nTo mitigate possible issues, the animations' settings will change, but this is not supported.", MessageType.Warning, true);
                EditorGUILayout.HelpBox("You can change the Write Defaults mode in 'Extra'", MessageType.Info, true);

                ((Installer)target).writeDefaultEnabled = true;
            }
            else
            {
                ((Installer)target).writeDefaultEnabled = false;
            }

        }


    }
}

#endif