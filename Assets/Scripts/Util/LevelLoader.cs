using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class LevelData
{
    public int goal;
    public int move;
    public int width;
    public int height;
    public int[] map; // 0 = 빈칸, 1 = 사용가능 칸
    public int[] colors; // 0 = 빨, 1 = 주, 2 = 노, 3 = 초, 4 = 파, 5 = 보, 6 = 박스 (영상 기준)
}

public class LevelLoader
{
    public static LevelData LoadLevel(string levelName)
    {
        // Resources/Levels/Level1.json
        TextAsset file = Resources.Load<TextAsset>($"Levels/{levelName}");
        if (file == null)
        {
            Debug.LogError($"레벨 파일 {levelName} 을(를) 찾을 수 없음!");
            return null;
        }

        return JsonUtility.FromJson<LevelData>(file.text);
    }
}
