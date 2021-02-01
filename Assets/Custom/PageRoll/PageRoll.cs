using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PageRoll : MonoBehaviour
{
    public int pageCount
    {
        get
        {
            return Mathf.FloorToInt(pageTexs.Length / 2);
        }
    }
    public int pageResCount
    {
        get
        {
            return pages.Length;
        }
    }
    public Texture[] pageTexs;
    public float rollSpeed = 1.0f;
    public float rollCost = 2.0f;
    public MeshRenderer[] pages;
    public float dragSpeed = 10.0f;
    public bool enableDrag = true;
    public float dragPower = 1.0f;

    private int curPage = 0;
    private HashSet<int> rollingPages = new HashSet<int>();
    private HashSet<int> rollingPages2 = new HashSet<int>();

    private List<MeshRenderer> pagesList = new List<MeshRenderer>();
    private bool isRolling = false;
    List<int> left = new List<int>();
    List<int> right = new List<int>();

    void Start()
    {
        for (int i = 0; i < pageCount; i++)
        {
            var pageIdx = i - Mathf.FloorToInt(i / pageResCount) * pageResCount;
            var page = pages[pageIdx];
            pagesList.Add(page);
        }
        for (int i = 0; i < pageResCount; i++)
        {
            var idx = i;
            var page = pages[idx];
            var targetIdx = GetTextureIdx(idx);
            PageData data = new PageData(pageTexs[2 * targetIdx], pageTexs[2 * targetIdx + 1], 180);
            RefreshPage(page, data);
            right.Add(idx);
        }
        FixPages();
    }

    private int GetTextureIdx(int idx)
    {
        return idx;
    }

    private void RefreshPage(MeshRenderer page, PageData data)
    {
        page.material.SetFloat("_Angle", data.angle);
        page.material.SetTexture("_MainTex", data.main);
        page.material.SetTexture("_SecondTex", data.second);
    }


    private void FixPages()
    {
        left.Sort();
        right.Sort();
        for (int i = 0; i < left.Count; i++)
        {
            var page = pagesList[left[i]];
            page.material.renderQueue = 2000 + i;
        }
        for (int i = right.Count - 1; i >= 0; i--)
        {
            var page = pagesList[right[i]];
            page.material.renderQueue = 2000 + right.Count - 1 - i;
        }
    }

    private void MovePages(bool isRight = false)
    {
        var idx = isRight ? curPage - 2 : curPage + 1;
        if (idx < 0 || idx >= pagesList.Count)
            return;

        var angle = isRight ? 0 : 180;
        var page = pagesList[idx];
        if (isRight)
        {
            if (page.Equals(pages[pages.Length - 1]))
            {
                if (right.Contains(idx))
                    right.Remove(idx);
                if (!left.Contains(idx))
                    left.Add(idx);
            }
        }
        else
        {
            if (page.Equals(pages[0]))
            {
                if (left.Contains(idx))
                    left.Remove(idx);
                if (!right.Contains(idx))
                    right.Add(idx);
            }
        }
        var targetIdx = GetTextureIdx(idx);
        PageData data = new PageData(pageTexs[2 * targetIdx], pageTexs[2 * targetIdx + 1], angle);
        RefreshPage(page, data);
    }

    private void LateUpdate()
    {
        List<int> discardRollingPages = new List<int>();
        List<int> discardRollingPages2 = new List<int>();
        foreach (var pageNum in rollingPages)
        {
            var page = pagesList[pageNum];
            var angle = page.material.GetFloat("_Angle");
            if (angle < 0.1)
            {
                page.material.SetFloat("_Angle", 0);
                discardRollingPages.Add(pageNum);
                if (!left.Contains(pageNum))
                    left.Add(pageNum);
                if (right.Contains(pageNum))
                    right.Remove(pageNum);
                curPage++;
            }
        }
        foreach (var pageNum in rollingPages2)
        {
            var page = pagesList[pageNum];
            var angle = page.material.GetFloat("_Angle");
            if (Mathf.Abs(180 - angle) < 0.1)
            {
                page.material.SetFloat("_Angle", 180);
                discardRollingPages2.Add(pageNum);
                if (!right.Contains(pageNum))
                    right.Add(pageNum);
                if (left.Contains(pageNum))
                    left.Remove(pageNum);
                curPage--;
            }
        }
        discardRollingPages.ForEach(pageNum =>
        {
            rollingPages.Remove(pageNum);
        });
        discardRollingPages2.ForEach(pageNum =>
        {
            rollingPages2.Remove(pageNum);
        });

        if (isRolling && !IsRolling())
            FixPages();
    }

    void CheckInput()
    {
        if (!enableDrag)
            return;
        var input_x = Input.GetAxis("Mouse X");
        if (Mathf.Abs(input_x)<dragPower)
            return;
        if (IsRolling())
        {
            int targetPageNum = -1;
            foreach (var num in rollingPages)
            {
                targetPageNum = num;
                rollingPages.Remove(curPage);
                break;
            }
            if (targetPageNum < 0)
            {
                foreach (var num in rollingPages2)
                {
                    targetPageNum = num;
                    rollingPages2.Remove(curPage);
                    break;
                }
            }
            if (targetPageNum < 0)
            {
                MeshRenderer page = pagesList[targetPageNum];
            }
        }
  
        //if (input_x > 0)
        //{
        //    RollRight();
        //}
        //else
        //{
        //    RollLeft();
        //}
    }

    void Update()
    {
        CheckInput();

        var delta = Time.deltaTime * rollSpeed;
        foreach (var pageNum in rollingPages)
        {
            var page = pagesList[pageNum];
            var angle = page.material.GetFloat("_Angle");
            var targetAngle = Mathf.Lerp(angle, 0, delta / rollCost);
            page.material.SetFloat("_Angle", targetAngle);
        }
        foreach (var pageNum in rollingPages2)
        {
            var page = pagesList[pageNum];
            var angle = page.material.GetFloat("_Angle");
            var targetAngle = Mathf.Lerp(angle, 180, delta / rollCost);
            page.material.SetFloat("_Angle", targetAngle);
        }
        isRolling = IsRolling();
    }

    private bool IsRollingLeft()
    {
        return rollingPages.Count > 0;
    }
    private bool IsRollingRight()
    {
        return rollingPages2.Count > 0;
    }
    private bool IsRolling()
    {
        return IsRollingLeft() || IsRollingRight();
    }

    public void RollLeft()
    {
        if (IsRolling())
            return;
        if (curPage >= pageCount)//pages.Length)
            return;
        MovePages();
        if (rollingPages2.Contains(curPage))
        {
            rollingPages2.Remove(curPage);
        }
        rollingPages.Add(curPage);
        //curPage++;
    }

    public void RollRight()
    {
        if (IsRolling())
            return;
        if (curPage <= 0)
            return;
        MovePages(true);
        if (rollingPages.Contains(curPage - 1))
        {
            rollingPages.Remove(curPage - 1);
        }
        rollingPages2.Add(curPage - 1);
        //curPage--;
    }

}

public struct PageData
{
    public Texture main;
    public Texture second;
    public float angle;


    public PageData(Texture main, Texture second, float angle)
    {
        this.main = main;
        this.second = second;
        this.angle = angle;
    }
}
