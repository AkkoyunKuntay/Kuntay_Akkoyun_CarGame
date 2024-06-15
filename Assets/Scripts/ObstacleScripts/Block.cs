using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Block : MonoBehaviour
{
    [SerializeField] private Vector2Int blockSize;
    private Vector2Int GetBlockSize()
    {
        blockSize.x = (int)transform.localScale.x;
        blockSize.y = (int)transform.localScale.z;
        return blockSize;
    }
    public void RenameByBlockSize()
    {
        Vector2Int size = GetBlockSize();
        gameObject.name = "Block" + size.x.ToString() + "x" + size.y.ToString();
    }
}
