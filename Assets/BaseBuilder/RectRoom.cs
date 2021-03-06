using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RectRoom : Structure
{
    [Range(1,5)]
    public int x;
    [Range(1,5)]
    public int y;
    [Range(1,5)]
    public int z;

    Vector3 size = new Vector3(10, 5, 10);

    public Transform wallLeft;
    public Transform wallRight;
    public Transform wallFront;
    public Transform wallBack;
    public Transform floor;
    public Transform ceiling;

    public AirLock door;

    void Start()
    {
        Generate();
    }
    void OnValidate(){
        Generate();
    }
    void Generate(){

        float wallThicc = .25f;

        float sizeX = x * size.x;
        float sizeY = y * size.y;
        float sizeZ = z * size.z;

        float wallY = sizeY / 2;
        float rightX = sizeX / 2;
        float frontZ = sizeZ / 2;

        floor.localPosition = new Vector3(0,0,0);
        floor.localScale = new Vector3(sizeX, wallThicc, sizeZ);

        ceiling.localScale = floor.localScale;
        ceiling.localPosition = new Vector3(0,sizeY,0);

        door.transform.localPosition = new Vector3(0, wallY, frontZ);
        door.transform.localScale = new Vector3(2f, 3f, .35f);

        door.room = this;

         wallLeft.localPosition = new Vector3(-rightX, wallY, 0);
        wallRight.localPosition = new Vector3(+rightX, wallY, 0);
         wallBack.localPosition = new Vector3(0, wallY, -frontZ);
        wallFront.localPosition = new Vector3(0, wallY, +frontZ);

         wallLeft.localScale = new Vector3(wallThicc, sizeY, sizeZ);
        wallRight.localScale = wallLeft.localScale;

         wallBack.localScale = new Vector3(sizeX, sizeY, wallThicc);
        wallFront.localScale = wallBack.localScale;
    }
    public override PreviewStructure MakePreview(BuilderNode parent, BuilderNode.BuildDirection buildDirection)
    {
        PreviewStructure preview = base.MakePreview(parent, buildDirection);
        preview.transform.localScale = new Vector3(
            x * size.x,
            y * size.y,
            z * size.z
        );
        return preview;
    }
}
