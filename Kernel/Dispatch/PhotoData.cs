using System.Collections;
using System.Collections.Generic;

public class PhotoData
{
    public string photoName;
    public string fileName;
    public string obtainTime;
    public PhotoData(string photoName, string resUrl, string obtainTime)
    {
        this.photoName = photoName;
        this.fileName = resUrl;
        this.obtainTime = obtainTime;
    }
}
