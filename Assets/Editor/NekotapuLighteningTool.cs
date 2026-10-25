using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEditor;
using UnityEngine;

/// <summary>
/// 軽量化ツール（エディタ専用。ビルドには含まれない）。
/// ・画像：Emotions と装備品の補助マップの Max Size を 256 にする。
/// ・ゴーグルの FBX：Mesh Compression を High にする。
/// ・メッシュ圧縮：ビルドに入っている小さい 3D モデルの Mesh Compression を High（要確認のものは Medium）にする。
/// 設定の変更は TextureImporter / ModelImporter の API で行い、.meta を直接書き換えない。
/// 変更前の設定は tools/lightening_backup.json に保存し、「元に戻す」で復元できる。
/// すでに記録されている元の値は、あとから上書きしない。
/// </summary>
public static class NekotapuLighteningTool
{
    private const string MenuRoot = "ねこたぷ/軽量化/";
    private const int TargetMaxSize = 256;
    private const string WebGLPlatform = "WebGL";
    private const int ExpectedEmotion = 28;
    private const int ExpectedSupportMap = 41;
    private const int ExpectedGoggles = 1;
    private const int ExpectedMeshHigh = 24;
    private const int ExpectedMeshMedium = 60;

    private enum Kind { Emotion, SupportMap, Goggles, MeshCompress }

    private class Target
    {
        public Kind kind;
        public string path;
        public string note;   // 「要確認」の理由
        public ModelImporterMeshCompression desired = ModelImporterMeshCompression.High;
        public Target(Kind kind, string path, string note = null)
        {
            this.kind = kind;
            this.path = path;
            this.note = note;
        }
        public Target(Kind kind, string path, ModelImporterMeshCompression desired, string note = null)
        {
            this.kind = kind;
            this.path = path;
            this.desired = desired;
            this.note = note;
        }
    }

    [Serializable]
    private class BackupEntry
    {
        public string path;
        public string kind;
        public int defaultMaxSize;
        public bool webglOverridden;
        public int webglMaxSize;
        public int meshCompression;
    }

    [Serializable]
    private class BackupFile
    {
        public string createdAt;
        public List<BackupEntry> entries = new List<BackupEntry>();
    }

    // 第5号の調査（調査_画像とモデル.md）と第7号の調査にもとづく対象。パスで指定する。
    private static readonly Target[] Targets =
    {
        // Suriyun Emotions 28 枚（ねこの表情）
        new Target(Kind.Emotion, "Assets/AssetsOld/Suriyun/Pspsps/Textures/Emotions/T_Chibi_Emo_01.png"),
        new Target(Kind.Emotion, "Assets/AssetsOld/Suriyun/Pspsps/Textures/Emotions/T_Chibi_Emo_02.png"),
        new Target(Kind.Emotion, "Assets/AssetsOld/Suriyun/Pspsps/Textures/Emotions/T_Chibi_Emo_03.png"),
        new Target(Kind.Emotion, "Assets/AssetsOld/Suriyun/Pspsps/Textures/Emotions/T_Chibi_Emo_04.png"),
        new Target(Kind.Emotion, "Assets/AssetsOld/Suriyun/Pspsps/Textures/Emotions/T_Chibi_Emo_05.png"),
        new Target(Kind.Emotion, "Assets/AssetsOld/Suriyun/Pspsps/Textures/Emotions/T_Chibi_Emo_06.png"),
        new Target(Kind.Emotion, "Assets/AssetsOld/Suriyun/Pspsps/Textures/Emotions/T_Chibi_Emo_07.png"),
        new Target(Kind.Emotion, "Assets/AssetsOld/Suriyun/Pspsps/Textures/Emotions/T_Chibi_Emo_08.png"),
        new Target(Kind.Emotion, "Assets/AssetsOld/Suriyun/Pspsps/Textures/Emotions/T_Chibi_Emo_09.png"),
        new Target(Kind.Emotion, "Assets/AssetsOld/Suriyun/Pspsps/Textures/Emotions/T_Chibi_Emo_10.png"),
        new Target(Kind.Emotion, "Assets/AssetsOld/Suriyun/Pspsps/Textures/Emotions/T_Chibi_Emo_11.png"),
        new Target(Kind.Emotion, "Assets/AssetsOld/Suriyun/Pspsps/Textures/Emotions/T_Chibi_Emo_12.png"),
        new Target(Kind.Emotion, "Assets/AssetsOld/Suriyun/Pspsps/Textures/Emotions/T_Chibi_Emo_13.png"),
        new Target(Kind.Emotion, "Assets/AssetsOld/Suriyun/Pspsps/Textures/Emotions/T_Chibi_Emo_14.png"),
        new Target(Kind.Emotion, "Assets/AssetsOld/Suriyun/Pspsps/Textures/Emotions/T_Chibi_Emo_15.png"),
        new Target(Kind.Emotion, "Assets/AssetsOld/Suriyun/Pspsps/Textures/Emotions/T_Chibi_Emo_16.png"),
        new Target(Kind.Emotion, "Assets/AssetsOld/Suriyun/Pspsps/Textures/Emotions/T_Chibi_Emo_17.png"),
        new Target(Kind.Emotion, "Assets/AssetsOld/Suriyun/Pspsps/Textures/Emotions/T_Chibi_Emo_18.png"),
        new Target(Kind.Emotion, "Assets/AssetsOld/Suriyun/Pspsps/Textures/Emotions/T_Chibi_Emo_19.png"),
        new Target(Kind.Emotion, "Assets/AssetsOld/Suriyun/Pspsps/Textures/Emotions/T_Chibi_Emo_20.png"),
        new Target(Kind.Emotion, "Assets/AssetsOld/Suriyun/Pspsps/Textures/Emotions/T_Chibi_Emo_21.png"),
        new Target(Kind.Emotion, "Assets/AssetsOld/Suriyun/Pspsps/Textures/Emotions/T_Chibi_Emo_22.png"),
        new Target(Kind.Emotion, "Assets/AssetsOld/Suriyun/Pspsps/Textures/Emotions/T_Chibi_Emo_23.png"),
        new Target(Kind.Emotion, "Assets/AssetsOld/Suriyun/Pspsps/Textures/Emotions/T_Chibi_Emo_24.png"),
        new Target(Kind.Emotion, "Assets/AssetsOld/Suriyun/Pspsps/Textures/Emotions/T_Chibi_Emo_25.png"),
        new Target(Kind.Emotion, "Assets/AssetsOld/Suriyun/Pspsps/Textures/Emotions/T_Chibi_Emo_26.png"),
        new Target(Kind.Emotion, "Assets/AssetsOld/Suriyun/Pspsps/Textures/Emotions/T_Chibi_Emo_27.png"),
        new Target(Kind.Emotion, "Assets/AssetsOld/Suriyun/Pspsps/Textures/Emotions/T_Chibi_Emo_28.png"),
        // 装備品・小物（AssetsForItems）の補助マップ 41 枚（法線・金属・AO・高さ・発光）
        // ※スマホ・パソコンの画面の絵（smartphone_n/_s/_screen、HQ Laptop の e.tga）は、見た目に注意のため対象から外した
        new Target(Kind.SupportMap, "Assets/AssetsForItems/Baseball Essentials/Assets/Textures/Glove/Variation 1/Glove_01_Ambient_Occlusion.png"),
        new Target(Kind.SupportMap, "Assets/AssetsForItems/Baseball Essentials/Assets/Textures/Glove/Variation 1/Glove_01_Metallic_Smoothness.png"),
        new Target(Kind.SupportMap, "Assets/AssetsForItems/Baseball Essentials/Assets/Textures/Glove/Variation 1/Glove_01_Normal.png"),
        new Target(Kind.SupportMap, "Assets/AssetsForItems/Baseball Essentials/Assets/Textures/Glove/Variation 2/Glove_02_Ambient_Occlusion.png"),
        new Target(Kind.SupportMap, "Assets/AssetsForItems/Baseball Essentials/Assets/Textures/Glove/Variation 2/Glove_02_Metallic_Smoothness.png"),
        new Target(Kind.SupportMap, "Assets/AssetsForItems/Baseball Essentials/Assets/Textures/Glove/Variation 2/Glove_02_Normal.png"),
        new Target(Kind.SupportMap, "Assets/AssetsForItems/Christmas/Blink_Christmas_2021/Props_Christmas/Props_Textures/Showman_Bake_AO.png"),
        new Target(Kind.SupportMap, "Assets/AssetsForItems/Christmas/Blink_Christmas_2021/Props_Christmas/Props_Textures/Showman_Bake_Normal.png"),
        new Target(Kind.SupportMap, "Assets/AssetsForItems/Christmas/Blink_Christmas_2021/Weapon_Christmas/ChristmasSword_Emissive.png"),
        new Target(Kind.SupportMap, "Assets/AssetsForItems/Christmas/Blink_Christmas_2021/Weapon_Christmas/ChristmasSword_Metallic.png"),
        new Target(Kind.SupportMap, "Assets/AssetsForItems/Christmas/Blink_Christmas_2021/Weapon_Christmas/ChristmasSword_Normal.png"),
        new Target(Kind.SupportMap, "Assets/AssetsForItems/Classic Ping-Pong Racket/Materials/Racket Mat/Racket Tex/Ball Metallic.png"),
        new Target(Kind.SupportMap, "Assets/AssetsForItems/Classic Ping-Pong Racket/Materials/Racket Mat/Racket Tex/Ball Normal.png"),
        new Target(Kind.SupportMap, "Assets/AssetsForItems/Classic Ping-Pong Racket/Materials/Racket Mat/Racket Tex/Racket Metallic.png"),
        new Target(Kind.SupportMap, "Assets/AssetsForItems/Classic Ping-Pong Racket/Materials/Racket Mat/Racket Tex/Racket Normal.png"),
        new Target(Kind.SupportMap, "Assets/AssetsForItems/Classic Ping-Pong Racket/Materials/Racket Mat/Racket Tex/Wood1 Metallic.png"),
        new Target(Kind.SupportMap, "Assets/AssetsForItems/Classic Ping-Pong Racket/Materials/Racket Mat/Racket Tex/Wood1 Normal.png"),
        new Target(Kind.SupportMap, "Assets/AssetsForItems/Classic Ping-Pong Racket/Materials/Racket Mat/Racket Tex/Wood2 Metallic.png"),
        new Target(Kind.SupportMap, "Assets/AssetsForItems/Classic Ping-Pong Racket/Materials/Racket Mat/Racket Tex/Wood2 Normal.png"),
        new Target(Kind.SupportMap, "Assets/AssetsForItems/Clipboard/Textures/clipboard2_board_Normal.png"),
        new Target(Kind.SupportMap, "Assets/AssetsForItems/Clipboard/Textures/clipboard2_metal_MetallicSmoothness.png"),
        new Target(Kind.SupportMap, "Assets/AssetsForItems/Clipboard/Textures/clipboard2_metal_Normal.png"),
        new Target(Kind.SupportMap, "Assets/AssetsForItems/Clipboard/Textures/clipboard2_page_Normal.png"),
        new Target(Kind.SupportMap, "Assets/AssetsForItems/FreeSportsKIt_SA/Textures/SoccerBall_textures/Ball_Normals.png"),
        new Target(Kind.SupportMap, "Assets/AssetsForItems/FreeSportsKIt_SA/Textures/basketBall_textures/Ball_Normal_OpenGL.png"),
        new Target(Kind.SupportMap, "Assets/AssetsForItems/FreeSportsKIt_SA/Textures/tennis_balltextures/tennis_ball2_Height.png"),
        new Target(Kind.SupportMap, "Assets/AssetsForItems/FreeSportsKIt_SA/Textures/tennis_balltextures/tennis_ball2_Metalness.png"),
        new Target(Kind.SupportMap, "Assets/AssetsForItems/FreeSportsKIt_SA/Textures/tennis_balltextures/tennis_ball2_Normal.png"),
        new Target(Kind.SupportMap, "Assets/AssetsForItems/FreeSportsKIt_SA/Textures/tennis_bat_textures/blue/tennis_bat2_Height.png"),
        new Target(Kind.SupportMap, "Assets/AssetsForItems/FreeSportsKIt_SA/Textures/tennis_bat_textures/blue/tennis_bat2_Metallic.png"),
        new Target(Kind.SupportMap, "Assets/AssetsForItems/FreeSportsKIt_SA/Textures/tennis_bat_textures/blue/tennis_bat2_Normal.png"),
        new Target(Kind.SupportMap, "Assets/AssetsForItems/FreeSportsKIt_SA/Textures/volleyball_textures/fbx_volleyball_aiStandardSurface1_Height.png"),
        new Target(Kind.SupportMap, "Assets/AssetsForItems/FreeSportsKIt_SA/Textures/volleyball_textures/fbx_volleyball_aiStandardSurface1_Metalness.png"),
        new Target(Kind.SupportMap, "Assets/AssetsForItems/FreeSportsKIt_SA/Textures/volleyball_textures/fbx_volleyball_aiStandardSurface1_Normal.png"),
        new Target(Kind.SupportMap, "Assets/AssetsForItems/HQ Laptop/Textures/normal.tga"),
        new Target(Kind.SupportMap, "Assets/AssetsForItems/HQ Laptop/Textures/specular.tga"),
        new Target(Kind.SupportMap, "Assets/AssetsForItems/Smartphone/smartphone_ao.tga"),
        new Target(Kind.SupportMap, "Assets/AssetsForItems/ToonyTurtle/Materials/Turtle_AO.tga"),
        new Target(Kind.SupportMap, "Assets/AssetsForItems/ToonyTurtle/Materials/Turtle_Height.tga"),
        new Target(Kind.SupportMap, "Assets/AssetsForItems/ToonyTurtle/Materials/Turtle_MetallicSmoothness.tga"),
        new Target(Kind.SupportMap, "Assets/AssetsForItems/ToonyTurtle/Materials/Turtle_Normal.tga"),
        // スチームパンクゴーグル
        new Target(Kind.Goggles, "Assets/Prefabs/Steampunk Goggles Wire_by IVL.fbx"),
        // メッシュ圧縮：High にするもの（ゴーグルで −69% だったため）
        new Target(Kind.MeshCompress, "Assets/AssetsForItems/Christmas/Blink_Christmas_2021/Props_Christmas/Showman/Showman.fbx", ModelImporterMeshCompression.High),
        new Target(Kind.MeshCompress, "Assets/AssetsForItems/Christmas/Blink_Christmas_2021/Weapon_Christmas/ChristmasSword.fbx", ModelImporterMeshCompression.High),
        new Target(Kind.MeshCompress, "Assets/AssetsForItems/Donut Set/Models/Donut.fbx", ModelImporterMeshCompression.High),
        new Target(Kind.MeshCompress, "Assets/AssetsForItems/Donut Set/Models/Glaze.fbx", ModelImporterMeshCompression.High),
        new Target(Kind.MeshCompress, "Assets/AssetsForItems/Donut Set/Models/SprinklesGlaze.fbx", ModelImporterMeshCompression.High),
        new Target(Kind.MeshCompress, "Assets/AssetsForItems/FreeSportsKIt_SA/Models/SoccerBall_01.fbx", ModelImporterMeshCompression.High),
        new Target(Kind.MeshCompress, "Assets/AssetsForItems/FreeSportsKIt_SA/Models/basketBall_mdl.fbx", ModelImporterMeshCompression.High),
        new Target(Kind.MeshCompress, "Assets/AssetsForItems/FreeSportsKIt_SA/Models/tennis_ball.fbx", ModelImporterMeshCompression.High),
        new Target(Kind.MeshCompress, "Assets/AssetsForItems/FreeSportsKIt_SA/Models/tennis_bat.fbx", ModelImporterMeshCompression.High),
        new Target(Kind.MeshCompress, "Assets/AssetsForItems/FreeSportsKIt_SA/Models/volleyball.fbx", ModelImporterMeshCompression.High),
        new Target(Kind.MeshCompress, "Assets/AssetsOld/Dice_6/Meshes/Dice_d6.fbx", ModelImporterMeshCompression.High),
        new Target(Kind.MeshCompress, "Assets/AssetsOld/FantasyEnvironments/Environments/Ambient-Occlusion-Trees/FBX/Oak_tree1.fbx", ModelImporterMeshCompression.High),
        new Target(Kind.MeshCompress, "Assets/AssetsOld/LittleFriends-CartoonAnimals-Lite/Meshes/StrawberrySpongeCake.fbx", ModelImporterMeshCompression.High),
        new Target(Kind.MeshCompress, "Assets/AssetsOld/Suriyun/Pspsps/FBX/Accessory/Dried_fish.fbx", ModelImporterMeshCompression.High),
        new Target(Kind.MeshCompress, "Assets/AssetsOld/Suriyun/Pspsps/FBX/Accessory/GrassA_Red.fbx", ModelImporterMeshCompression.High),
        new Target(Kind.MeshCompress, "Assets/AssetsOld/Suriyun/Pspsps/FBX/Accessory/MeatA.fbx", ModelImporterMeshCompression.High),
        new Target(Kind.MeshCompress, "Assets/AssetsOld/Suriyun/Pspsps/FBX/Accessory/Pan.fbx", ModelImporterMeshCompression.High),
        new Target(Kind.MeshCompress, "Assets/AssetsOld/Suriyun/Pspsps/FBX/Accessory/SpadA.fbx", ModelImporterMeshCompression.High),
        new Target(Kind.MeshCompress, "Assets/AssetsOld/Suriyun/Pspsps/FBX/Environment/Floor_A.fbx", ModelImporterMeshCompression.High),
        new Target(Kind.MeshCompress, "Assets/AssetsOld/Suriyun/Pspsps/FBX/Environment/Mushroom_A.fbx", ModelImporterMeshCompression.High),
        new Target(Kind.MeshCompress, "Assets/Prefabs/Glasses.blend", ModelImporterMeshCompression.High),
        new Target(Kind.MeshCompress, "Assets/Prefabs/Quest2_lowpoly.fbx", ModelImporterMeshCompression.High),
        new Target(Kind.MeshCompress, "Assets/Prefabs/uploads_files_2808886_Scifi+Mono+Lens.blend", ModelImporterMeshCompression.High),
        new Target(Kind.MeshCompress, "Assets/Prefabs/uploads_files_313750_SunGlasses1.obj", ModelImporterMeshCompression.High),
        // メッシュ圧縮：要確認のため Medium にするもの（ボーン付き・キャラクター・Read/Write 有効・MeshCollider 使用など）
        new Target(Kind.MeshCompress, "Assets/AssetsForItems/Baseball Essentials/Assets/Meshes/Glove.obj", ModelImporterMeshCompression.Medium, "Read/Write 有効"),
        new Target(Kind.MeshCompress, "Assets/AssetsForItems/Baseball Essentials/Assets/Meshes/Glove_Collider.obj", ModelImporterMeshCompression.Medium, "Read/Write 有効／MeshCollider が使用"),
        new Target(Kind.MeshCompress, "Assets/AssetsForItems/Classic Ping-Pong Racket/Mesh/ClassicRacket.FBX", ModelImporterMeshCompression.Medium, "MeshCollider が使用"),
        new Target(Kind.MeshCompress, "Assets/AssetsForItems/Clipboard/Meshes/ClipBoard.fbx", ModelImporterMeshCompression.Medium, "Read/Write 有効"),
        new Target(Kind.MeshCompress, "Assets/AssetsForItems/HQ Laptop/Mesh/Laptop.FBX", ModelImporterMeshCompression.Medium, "Read/Write 有効"),
        new Target(Kind.MeshCompress, "Assets/AssetsForItems/Mg3D_Hats/CowboyHat.fbx", ModelImporterMeshCompression.Medium, "Read/Write 有効"),
        new Target(Kind.MeshCompress, "Assets/AssetsForItems/Mg3D_Hats/MagicianHat.fbx", ModelImporterMeshCompression.Medium, "Read/Write 有効"),
        new Target(Kind.MeshCompress, "Assets/AssetsForItems/Mg3D_Hats/MinerHat.fbx", ModelImporterMeshCompression.Medium, "Read/Write 有効"),
        new Target(Kind.MeshCompress, "Assets/AssetsForItems/Mg3D_Hats/PajamaHat.fbx", ModelImporterMeshCompression.Medium, "Read/Write 有効"),
        new Target(Kind.MeshCompress, "Assets/AssetsForItems/Mg3D_Hats/PoliceCap.fbx", ModelImporterMeshCompression.Medium, "Read/Write 有効"),
        new Target(Kind.MeshCompress, "Assets/AssetsForItems/Mg3D_Hats/ShowerCap.fbx", ModelImporterMeshCompression.Medium, "Read/Write 有効"),
        new Target(Kind.MeshCompress, "Assets/AssetsForItems/Mg3D_Hats/Sombrero.fbx", ModelImporterMeshCompression.Medium, "Read/Write 有効"),
        new Target(Kind.MeshCompress, "Assets/AssetsForItems/Mg3D_Hats/VikingHelmet.fbx", ModelImporterMeshCompression.Medium, "Read/Write 有効"),
        new Target(Kind.MeshCompress, "Assets/AssetsForItems/Office Supplies Low Poly/Assets/FBX/Eraser.fbx", ModelImporterMeshCompression.Medium, "Read/Write 有効"),
        new Target(Kind.MeshCompress, "Assets/AssetsForItems/Office Supplies Low Poly/Assets/FBX/Pencil red.fbx", ModelImporterMeshCompression.Medium, "Read/Write 有効／細い部品の可能性"),
        new Target(Kind.MeshCompress, "Assets/AssetsForItems/Smartphone/smartphone.FBX", ModelImporterMeshCompression.Medium, "Read/Write 有効"),
        new Target(Kind.MeshCompress, "Assets/AssetsForItems/Snowconesolid Assets/Super Rubber Duck Pack/Mesh/RubberDuck_Pink.fbx", ModelImporterMeshCompression.Medium, "Read/Write 有効"),
        new Target(Kind.MeshCompress, "Assets/AssetsForItems/Snowconesolid Assets/Super Rubber Duck Pack/Mesh/RubberDuck_Pond.fbx", ModelImporterMeshCompression.Medium, "Read/Write 有効"),
        new Target(Kind.MeshCompress, "Assets/AssetsForItems/Snowconesolid Assets/Super Rubber Duck Pack/Mesh/RubberDuck_Yellow.fbx", ModelImporterMeshCompression.Medium, "Read/Write 有効"),
        new Target(Kind.MeshCompress, "Assets/AssetsForItems/ToonyTurtle/Turtle.fbx", ModelImporterMeshCompression.Medium, "Read/Write 有効／ボーン・スキン付き"),
        new Target(Kind.MeshCompress, "Assets/AssetsOld/Baker House/Models/Baker_house.FBX", ModelImporterMeshCompression.Medium, "Read/Write 有効"),
        new Target(Kind.MeshCompress, "Assets/AssetsOld/FantasyEnvironments/Environments/Ambient-Occlusion-Trees/FBX/Birch_tree1.fbx", ModelImporterMeshCompression.Medium, "Read/Write 有効"),
        new Target(Kind.MeshCompress, "Assets/AssetsOld/FantasyEnvironments/Environments/Ambient-Occlusion-Trees/FBX/Deciduous_tree1.fbx", ModelImporterMeshCompression.Medium, "Read/Write 有効"),
        new Target(Kind.MeshCompress, "Assets/AssetsOld/FantasyEnvironments/Environments/FBX/Flower1.FBX", ModelImporterMeshCompression.Medium, "Read/Write 有効"),
        new Target(Kind.MeshCompress, "Assets/AssetsOld/FantasyEnvironments/Environments/FBX/Flower2.FBX", ModelImporterMeshCompression.Medium, "Read/Write 有効"),
        new Target(Kind.MeshCompress, "Assets/AssetsOld/FantasyEnvironments/Environments/FBX/Grass1.FBX", ModelImporterMeshCompression.Medium, "Read/Write 有効"),
        new Target(Kind.MeshCompress, "Assets/AssetsOld/FantasyEnvironments/Environments/FBX/Mushroom1.FBX", ModelImporterMeshCompression.Medium, "Read/Write 有効"),
        new Target(Kind.MeshCompress, "Assets/AssetsOld/FantasyEnvironments/Environments/FBX/Rock1.fbx", ModelImporterMeshCompression.Medium, "Read/Write 有効／MeshCollider が使用"),
        new Target(Kind.MeshCompress, "Assets/AssetsOld/LittleFriends-CartoonAnimals-Lite/Meshes/LittleCat.fbx", ModelImporterMeshCompression.Medium, "ボーン・スキン付き／キャラクター"),
        new Target(Kind.MeshCompress, "Assets/AssetsOld/Mushroom house/mesh/mushroom_house.FBX", ModelImporterMeshCompression.Medium, "Read/Write 有効"),
        new Target(Kind.MeshCompress, "Assets/AssetsOld/RPGMonsterBuddiesPBRPA/Animation/Mushroom/Mushroom_Attack01Smile.fbx", ModelImporterMeshCompression.Medium, "ボーン・スキン付き／キャラクター"),
        new Target(Kind.MeshCompress, "Assets/AssetsOld/RPGMonsterBuddiesPBRPA/Animation/Mushroom/Mushroom_Attack02Smile.fbx", ModelImporterMeshCompression.Medium, "ボーン・スキン付き／キャラクター"),
        new Target(Kind.MeshCompress, "Assets/AssetsOld/RPGMonsterBuddiesPBRPA/Animation/Mushroom/Mushroom_Attack03Smile.fbx", ModelImporterMeshCompression.Medium, "ボーン・スキン付き／キャラクター"),
        new Target(Kind.MeshCompress, "Assets/AssetsOld/RPGMonsterBuddiesPBRPA/Animation/Mushroom/Mushroom_DieSmile.fbx", ModelImporterMeshCompression.Medium, "ボーン・スキン付き／キャラクター"),
        new Target(Kind.MeshCompress, "Assets/AssetsOld/RPGMonsterBuddiesPBRPA/Animation/Mushroom/Mushroom_DizzySmile.fbx", ModelImporterMeshCompression.Medium, "ボーン・スキン付き／キャラクター"),
        new Target(Kind.MeshCompress, "Assets/AssetsOld/RPGMonsterBuddiesPBRPA/Animation/Mushroom/Mushroom_GetHitSmile.fbx", ModelImporterMeshCompression.Medium, "ボーン・スキン付き／キャラクター"),
        new Target(Kind.MeshCompress, "Assets/AssetsOld/RPGMonsterBuddiesPBRPA/Animation/Mushroom/Mushroom_IdleBattleSmile.fbx", ModelImporterMeshCompression.Medium, "ボーン・スキン付き／キャラクター"),
        new Target(Kind.MeshCompress, "Assets/AssetsOld/RPGMonsterBuddiesPBRPA/Animation/Mushroom/Mushroom_IdleNormalSmile.fbx", ModelImporterMeshCompression.Medium, "ボーン・スキン付き／キャラクター"),
        new Target(Kind.MeshCompress, "Assets/AssetsOld/RPGMonsterBuddiesPBRPA/Animation/Mushroom/Mushroom_IdlePlant.fbx", ModelImporterMeshCompression.Medium, "ボーン・スキン付き／キャラクター"),
        new Target(Kind.MeshCompress, "Assets/AssetsOld/RPGMonsterBuddiesPBRPA/Animation/Mushroom/Mushroom_IdlePlantToBattleSmile.fbx", ModelImporterMeshCompression.Medium, "ボーン・スキン付き／キャラクター"),
        new Target(Kind.MeshCompress, "Assets/AssetsOld/RPGMonsterBuddiesPBRPA/Animation/Mushroom/Mushroom_SenseSomethingMaintainSmile.fbx", ModelImporterMeshCompression.Medium, "ボーン・スキン付き／キャラクター"),
        new Target(Kind.MeshCompress, "Assets/AssetsOld/RPGMonsterBuddiesPBRPA/Animation/Mushroom/Mushroom_SenseSomethingStartSmile.fbx", ModelImporterMeshCompression.Medium, "ボーン・スキン付き／キャラクター"),
        new Target(Kind.MeshCompress, "Assets/AssetsOld/RPGMonsterBuddiesPBRPA/Animation/Mushroom/Mushroom_TauntingSmile.fbx", ModelImporterMeshCompression.Medium, "ボーン・スキン付き／キャラクター"),
        new Target(Kind.MeshCompress, "Assets/AssetsOld/RPGMonsterBuddiesPBRPA/Animation/Mushroom/Mushroom_VictorySmile.fbx", ModelImporterMeshCompression.Medium, "ボーン・スキン付き／キャラクター"),
        new Target(Kind.MeshCompress, "Assets/AssetsOld/RPGMonsterBuddiesPBRPA/Animation/Mushroom/Mushroom_runFWDSmile.fbx", ModelImporterMeshCompression.Medium, "ボーン・スキン付き／キャラクター"),
        new Target(Kind.MeshCompress, "Assets/AssetsOld/RPGMonsterBuddiesPBRPA/Animation/Mushroom/Mushroom_walkBWDSmile.fbx", ModelImporterMeshCompression.Medium, "ボーン・スキン付き／キャラクター"),
        new Target(Kind.MeshCompress, "Assets/AssetsOld/RPGMonsterBuddiesPBRPA/Animation/Mushroom/Mushroom_walkFWDSmile.fbx", ModelImporterMeshCompression.Medium, "ボーン・スキン付き／キャラクター"),
        new Target(Kind.MeshCompress, "Assets/AssetsOld/RPGMonsterBuddiesPBRPA/Animation/Mushroom/Mushroom_walkLFTSmile.fbx", ModelImporterMeshCompression.Medium, "ボーン・スキン付き／キャラクター"),
        new Target(Kind.MeshCompress, "Assets/AssetsOld/RPGMonsterBuddiesPBRPA/Animation/Mushroom/Mushroom_walkRGTSmile.fbx", ModelImporterMeshCompression.Medium, "ボーン・スキン付き／キャラクター"),
        new Target(Kind.MeshCompress, "Assets/AssetsOld/RPGMonsterBuddiesPBRPA/Mesh/MushroomMeshPBR.fbx", ModelImporterMeshCompression.Medium, "ボーン・スキン付き／キャラクター"),
        new Target(Kind.MeshCompress, "Assets/AssetsOld/Suriyun/Pspsps/FBX/Accessory/Bag_bread.fbx", ModelImporterMeshCompression.Medium, "ボーン・スキン付き"),
        new Target(Kind.MeshCompress, "Assets/AssetsOld/Suriyun/Pspsps/FBX/Accessory/Baton_wood.fbx", ModelImporterMeshCompression.Medium, "ボーン・スキン付き"),
        new Target(Kind.MeshCompress, "Assets/AssetsOld/Suriyun/Pspsps/FBX/Accessory/Hiking_hat_A.fbx", ModelImporterMeshCompression.Medium, "ボーン・スキン付き"),
        new Target(Kind.MeshCompress, "Assets/AssetsOld/Suriyun/Pspsps/FBX/Accessory/Whirligig_A.fbx", ModelImporterMeshCompression.Medium, "ボーン・スキン付き"),
        new Target(Kind.MeshCompress, "Assets/AssetsOld/Suriyun/Pspsps/FBX/Characters/Chibi_Cat.fbx", ModelImporterMeshCompression.Medium, "ボーン・スキン付き／キャラクター"),
        new Target(Kind.MeshCompress, "Assets/AssetsOld/Suriyun/Pspsps/FBX/Environment/Butterfly_A.fbx", ModelImporterMeshCompression.Medium, "ボーン・スキン付き"),
        new Target(Kind.MeshCompress, "Assets/AssetsOld/Suriyun/Pspsps/FBX/Environment/Flower_A.fbx", ModelImporterMeshCompression.Medium, "ボーン・スキン付き"),
        new Target(Kind.MeshCompress, "Assets/AssetsOld/Suriyun/Pspsps/FBX/Environment/Grass_A.fbx", ModelImporterMeshCompression.Medium, "ボーン・スキン付き"),
        new Target(Kind.MeshCompress, "Assets/AssetsOld/Suriyun/Pspsps/FBX/Environment/Tree_A.fbx", ModelImporterMeshCompression.Medium, "ボーン・スキン付き"),
        new Target(Kind.MeshCompress, "Assets/AssetsOld/VRHands/Models/HandsVR.fbx", ModelImporterMeshCompression.Medium, "Read/Write 有効／ボーン・スキン付き／キャラクター"),
        // ※アニメーション専用の FBX（Animations フォルダ内、または名前に @ を含むもの）63 件は、メッシュ圧縮の効果が見込めないため対象外
    };

    private static string BackupPath
    {
        get
        {
            string projectRoot = Directory.GetParent(Application.dataPath).FullName;
            return Path.Combine(projectRoot, "tools", "lightening_backup.json");
        }
    }

    private static bool IsImageOrGoggles(Target t) { return t.kind != Kind.MeshCompress; }
    private static bool IsMesh(Target t) { return t.kind == Kind.MeshCompress; }

    [MenuItem(MenuRoot + "対象の一覧（確認のみ）")]
    private static void ListTargets()
    {
        var sb = new StringBuilder();
        var counts = new Dictionary<string, int>();
        int missing = 0, check = 0;
        sb.AppendLine("軽量化の対象の一覧（確認のみ。何も変更していません）");
        foreach (Target t in Targets)
        {
            string line = Describe(t, out bool exists);
            if (!exists) { missing++; }
            else
            {
                string key = t.kind == Kind.MeshCompress ? "Mesh" + t.desired : t.kind.ToString();
                counts[key] = counts.ContainsKey(key) ? counts[key] + 1 : 1;
            }
            if (t.note != null) { check++; }
            sb.AppendLine(line);
        }
        sb.AppendLine("--- 件数の確認 ---");
        sb.AppendLine(CountLine("Emotions", Get(counts, "Emotion"), ExpectedEmotion));
        sb.AppendLine(CountLine("補助マップ", Get(counts, "SupportMap"), ExpectedSupportMap));
        sb.AppendLine(CountLine("ゴーグル", Get(counts, "Goggles"), ExpectedGoggles));
        sb.AppendLine(CountLine("メッシュ圧縮（High）", Get(counts, "MeshHigh"), ExpectedMeshHigh));
        sb.AppendLine(CountLine("メッシュ圧縮（要確認→Medium）", Get(counts, "MeshMedium"), ExpectedMeshMedium));
        if (missing > 0) { sb.AppendLine($"見つからないファイル：{missing} 個（パスが変わった可能性があります）"); }
        sb.AppendLine($"要確認（見た目に注意）：{check} 個");
        sb.AppendLine(File.Exists(BackupPath)
            ? $"バックアップ：あり（{BackupPath}）"
            : "バックアップ：まだなし（実行の前に自動で作ります）");
        Debug.Log(sb.ToString());
    }

    [MenuItem(MenuRoot + "軽量化を実行（画像とゴーグル）")]
    private static void RunImages()
    {
        Run(IsImageOrGoggles, "画像とゴーグル",
            $"・画像：Max Size を {TargetMaxSize} にする（すでに {TargetMaxSize} 以下のものは変えない）\n" +
            "・ゴーグルの FBX：Mesh Compression を High にする");
    }

    [MenuItem(MenuRoot + "軽量化を実行（メッシュ圧縮）")]
    private static void RunMesh()
    {
        Run(IsMesh, "メッシュ圧縮",
            "・小さい 3D モデルの Mesh Compression を High にする\n" +
            "・「要確認」のもの（ボーン付き・キャラクター・Read/Write 有効など）は Medium にする\n" +
            "・すでに同じ設定のものは変えない");
    }

    [MenuItem(MenuRoot + "元に戻す（すべて）")]
    private static void RestoreAll()
    {
        Restore(e => true, "すべて");
    }

    [MenuItem(MenuRoot + "元に戻す（メッシュ圧縮だけ）")]
    private static void RestoreMeshOnly()
    {
        Restore(e => e.kind == Kind.MeshCompress.ToString(), "メッシュ圧縮だけ");
    }

    private static void Run(Func<Target, bool> filter, string label, string description)
    {
        var targets = new List<Target>();
        int check = 0;
        foreach (Target t in Targets)
        {
            if (!filter(t)) { continue; }
            if (AssetImporter.GetAtPath(t.path) == null) { continue; }
            targets.Add(t);
            if (t.note != null) { check++; }
        }
        if (targets.Count == 0)
        {
            EditorUtility.DisplayDialog("ねこたぷ 軽量化", "対象のファイルが見つかりません。", "OK");
            return;
        }

        bool ok = EditorUtility.DisplayDialog(
            "ねこたぷ 軽量化（" + label + "）",
            $"{targets.Count} 個のファイルの設定を変えます。\n" +
            description + "\n" +
            $"（「見た目に注意」の対象 {check} 個を含みます）\n\n" +
            "変える前の設定は tools/lightening_backup.json に保存します。\n" +
            "メニューの「元に戻す」で元の設定に戻せます。",
            "実行する", "やめる");
        if (!ok) { return; }

        // 実行の前に、変える前の設定を保存する（すでにバックアップがあるものは、元の値を残す）。
        BackupFile backup = LoadBackup() ?? new BackupFile();
        if (string.IsNullOrEmpty(backup.createdAt)) { backup.createdAt = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"); }
        int added = 0;
        foreach (Target t in targets)
        {
            if (backup.entries.Exists(e => e.path == t.path)) { continue; }
            backup.entries.Add(Capture(t));
            added++;
        }
        SaveBackup(backup);

        var sb = new StringBuilder();
        int changed = 0;
        sb.AppendLine("軽量化を実行しました（" + label + "）：");
        AssetDatabase.StartAssetEditing();
        try
        {
            for (int i = 0; i < targets.Count; i++)
            {
                Target t = targets[i];
                EditorUtility.DisplayProgressBar("ねこたぷ 軽量化", t.path, (float)i / targets.Count);
                string result = Apply(t);
                if (result != null) { changed++; sb.AppendLine("  " + result); }
            }
        }
        finally
        {
            EditorUtility.ClearProgressBar();
            AssetDatabase.StopAssetEditing();
        }
        AssetDatabase.Refresh();
        sb.AppendLine($"変更したファイル：{changed} 個（変更不要：{targets.Count - changed} 個）。バックアップに新しく記録：{added} 件。{BackupPath}");
        Debug.Log(sb.ToString());
    }

    private static void Restore(Func<BackupEntry, bool> predicate, string label)
    {
        BackupFile backup = LoadBackup();
        if (backup == null || backup.entries.Count == 0)
        {
            EditorUtility.DisplayDialog("ねこたぷ 軽量化", "バックアップが見つかりません。\n" + BackupPath, "OK");
            return;
        }
        var selected = backup.entries.FindAll(e => predicate(e));
        if (selected.Count == 0)
        {
            EditorUtility.DisplayDialog("ねこたぷ 軽量化", "戻す対象がバックアップにありません（" + label + "）。", "OK");
            return;
        }

        bool ok = EditorUtility.DisplayDialog(
            "ねこたぷ 軽量化（元に戻す：" + label + "）",
            $"{selected.Count} 個のファイルを、バックアップ（{backup.createdAt}）の設定に戻します。",
            "戻す", "やめる");
        if (!ok) { return; }

        var sb = new StringBuilder();
        int restored = 0;
        AssetDatabase.StartAssetEditing();
        try
        {
            for (int i = 0; i < selected.Count; i++)
            {
                BackupEntry e = selected[i];
                EditorUtility.DisplayProgressBar("ねこたぷ 軽量化（元に戻す）", e.path, (float)i / selected.Count);
                string result = RestoreEntry(e);
                if (result != null) { restored++; sb.AppendLine("  " + result); }
            }
        }
        finally
        {
            EditorUtility.ClearProgressBar();
            AssetDatabase.StopAssetEditing();
        }
        AssetDatabase.Refresh();

        // 戻した分は restored のファイルへ移し、残りはバックアップに残す。
        string done = Path.Combine(Path.GetDirectoryName(BackupPath), "lightening_backup_restored.json");
        BackupFile doneFile = File.Exists(done) ? JsonUtility.FromJson<BackupFile>(File.ReadAllText(done, Encoding.UTF8)) : new BackupFile();
        if (string.IsNullOrEmpty(doneFile.createdAt)) { doneFile.createdAt = backup.createdAt; }
        foreach (BackupEntry e in selected)
        {
            doneFile.entries.RemoveAll(x => x.path == e.path);
            doneFile.entries.Add(e);
        }
        File.WriteAllText(done, JsonUtility.ToJson(doneFile, true), Encoding.UTF8);
        backup.entries.RemoveAll(e => selected.Contains(e));
        if (backup.entries.Count == 0) { File.Delete(BackupPath); }
        else { SaveBackup(backup); }
        Debug.Log($"元の設定に戻しました（{label}）：{restored} 個。戻した記録は {done} にあります。\n" + sb);
    }

    // ---- 設定の読み取り・変更 ----

    private static BackupEntry Capture(Target t)
    {
        var e = new BackupEntry { path = t.path, kind = t.kind.ToString() };
        AssetImporter importer = AssetImporter.GetAtPath(t.path);
        if (importer is TextureImporter ti)
        {
            e.defaultMaxSize = ti.maxTextureSize;
            TextureImporterPlatformSettings ps = ti.GetPlatformTextureSettings(WebGLPlatform);
            e.webglOverridden = ps.overridden;
            e.webglMaxSize = ps.maxTextureSize;
        }
        else if (importer is ModelImporter mi)
        {
            e.meshCompression = (int)mi.meshCompression;
        }
        return e;
    }

    // 変更したときだけ結果の文字列を返す。変更不要なら null。
    private static string Apply(Target t)
    {
        AssetImporter importer = AssetImporter.GetAtPath(t.path);
        if (importer is TextureImporter ti)
        {
            bool dirty = false;
            string before = $"Max {ti.maxTextureSize}";
            if (ti.maxTextureSize > TargetMaxSize)
            {
                ti.maxTextureSize = TargetMaxSize;
                dirty = true;
            }
            TextureImporterPlatformSettings ps = ti.GetPlatformTextureSettings(WebGLPlatform);
            if (ps.overridden && ps.maxTextureSize > TargetMaxSize)
            {
                before += $" / WebGL上書き {ps.maxTextureSize}";
                ps.maxTextureSize = TargetMaxSize;
                ti.SetPlatformTextureSettings(ps);
                dirty = true;
            }
            if (!dirty) { return null; }
            ti.SaveAndReimport();
            return $"{t.path} | {before} → {TargetMaxSize}";
        }
        if (importer is ModelImporter mi)
        {
            // すでに希望以上の圧縮なら変えない（Medium のものを High に下げない・High のものを Medium に下げない）
            if (mi.meshCompression == t.desired) { return null; }
            if (t.desired == ModelImporterMeshCompression.Medium && mi.meshCompression == ModelImporterMeshCompression.High) { return null; }
            string before = mi.meshCompression.ToString();
            mi.meshCompression = t.desired;
            mi.SaveAndReimport();
            return $"{t.path} | Mesh Compression {before} → {t.desired}";
        }
        return null;
    }

    private static string RestoreEntry(BackupEntry e)
    {
        AssetImporter importer = AssetImporter.GetAtPath(e.path);
        if (importer is TextureImporter ti)
        {
            bool dirty = false;
            if (ti.maxTextureSize != e.defaultMaxSize)
            {
                ti.maxTextureSize = e.defaultMaxSize;
                dirty = true;
            }
            TextureImporterPlatformSettings ps = ti.GetPlatformTextureSettings(WebGLPlatform);
            if (ps.overridden != e.webglOverridden || (e.webglOverridden && ps.maxTextureSize != e.webglMaxSize))
            {
                ps.overridden = e.webglOverridden;
                ps.maxTextureSize = e.webglMaxSize;
                ti.SetPlatformTextureSettings(ps);
                dirty = true;
            }
            if (!dirty) { return null; }
            ti.SaveAndReimport();
            return $"{e.path} | Max {e.defaultMaxSize}";
        }
        if (importer is ModelImporter mi)
        {
            if ((int)mi.meshCompression == e.meshCompression) { return null; }
            mi.meshCompression = (ModelImporterMeshCompression)e.meshCompression;
            mi.SaveAndReimport();
            return $"{e.path} | Mesh Compression {mi.meshCompression}";
        }
        return null;
    }

    private static string Describe(Target t, out bool exists)
    {
        AssetImporter importer = AssetImporter.GetAtPath(t.path);
        exists = importer != null;
        string mark = t.note != null ? "【要確認】" : "";
        string name = t.kind + " " + t.path;
        string note = t.note != null ? $" ※{t.note}" : "";
        if (importer is TextureImporter ti)
        {
            TextureImporterPlatformSettings ps = ti.GetPlatformTextureSettings(WebGLPlatform);
            string nowWebgl = ps.overridden ? $"WebGL上書き {ps.maxTextureSize}" : "WebGL上書きなし";
            int afterDefault = Mathf.Min(ti.maxTextureSize, TargetMaxSize);
            string afterWebgl = ps.overridden ? $"WebGL上書き {Mathf.Min(ps.maxTextureSize, TargetMaxSize)}" : "WebGL上書きなし";
            string state = (ti.maxTextureSize <= TargetMaxSize && (!ps.overridden || ps.maxTextureSize <= TargetMaxSize)) ? "（変更不要）" : "";
            return $"  {mark}{name} | 今：Max {ti.maxTextureSize}／{nowWebgl} → 後：Max {afterDefault}／{afterWebgl}{state}{note}";
        }
        if (importer is ModelImporter mi)
        {
            bool noChange = mi.meshCompression == t.desired
                || (t.desired == ModelImporterMeshCompression.Medium && mi.meshCompression == ModelImporterMeshCompression.High);
            string state = noChange ? "（変更不要）" : "";
            return $"  {mark}{name} | 今：Mesh Compression {mi.meshCompression} → 後：{t.desired}{state}{note}";
        }
        return $"  {name} | 見つかりません";
    }

    private static int Get(Dictionary<string, int> d, string key)
    {
        return d.ContainsKey(key) ? d[key] : 0;
    }

    private static string CountLine(string label, int actual, int expected)
    {
        return actual == expected
            ? $"{label}：{actual} 個（予定どおり）"
            : $"{label}：{actual} 個（予定は {expected} 個。数が合いません）";
    }

    // ---- バックアップの読み書き ----

    private static BackupFile LoadBackup()
    {
        if (!File.Exists(BackupPath)) { return null; }
        return JsonUtility.FromJson<BackupFile>(File.ReadAllText(BackupPath, Encoding.UTF8));
    }

    private static void SaveBackup(BackupFile backup)
    {
        Directory.CreateDirectory(Path.GetDirectoryName(BackupPath));
        File.WriteAllText(BackupPath, JsonUtility.ToJson(backup, true), Encoding.UTF8);
    }
}
