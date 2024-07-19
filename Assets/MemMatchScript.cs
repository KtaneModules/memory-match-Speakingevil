using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class MemMatchScript : MonoBehaviour {

    public KMAudio Audio;
    public KMNeedyModule module;
    public Transform[] objects;
    public List<KMSelectable> cards;
    public Renderer[] faces;
    public Material[] suits;

    private int[] sel = new int[6];
    private int[] arr = new int[12];
    private bool[] match = new bool[12];
    private int[] pick = new int[2] { -1, -1 };
    private bool pressable;

    private static int moduleIDCounter;
    private int moduleID;

    private void Awake()
    {
        moduleID = ++moduleIDCounter;
        module.OnNeedyActivation += Activate;
        module.OnNeedyDeactivation += Deactivate;
        for (int i = 0; i < 12; i++)
        {
            objects[i].localPosition = new Vector3(-0.0526f, 0.0121f + (i * 0.00022f), 0.0902f);
            objects[i].localEulerAngles = new Vector3(0, 90, 180);
        }
        foreach(KMSelectable card in cards)
        {
            int c = cards.IndexOf(card);
            card.OnInteract += delegate ()
            {
                if (pressable && !match[c] && !pick.Contains(c))
                {
                    Audio.PlaySoundAtTransform("Flip", card.transform);
                    StartCoroutine(Flip(c, true));
                }
                return false;
            };
        }
    }

    private void Activate()
    {
        sel = Enumerable.Range(0, 8).ToArray().Shuffle().Take(6).ToArray();
        for(int i = 0; i < 6; i++)
        {
            arr[i] = sel[i];
            arr[i + 6] = sel[i];
        }
        arr = arr.Shuffle();
        for (int i = 0; i < 12; i++)
        {
            match[i] = false;
            objects[i].localPosition = new Vector3(-0.0526f, 0.0121f + (i * 0.00022f), 0.0902f);
            objects[i].localEulerAngles = new Vector3(0, 90, 180);
            faces[i].material = suits[arr[i]];
        }
        string d = "The pairs are dealt in the following positions: ";
        for (int i = 0; i < 6; i++)
        {
            string p = "";
            for (int j = 0; j < 12; j++)
            {
                if (arr[j] == sel[i])
                {
                    string c = "ABCD"[j % 4] + ((j / 4) + 1).ToString();
                    if (p == "")
                        p = c + "-";
                    else
                    {
                        p += c;
                        if(i < 5)
                            p += ", ";
                    }
                }
            }
            d += p;
        }
        Debug.LogFormat("[Memory Match #{0}] {1}", moduleID, d);
        StartCoroutine(Deal(true));
    }
    private void Deactivate()
    {
        for(int i = 0; i < 12; i++)
            if (!match[i] && !pick.Contains(i))
            {
                Debug.LogFormat("[Memory Match #{0}] Times up!", moduleID);
                module.HandleStrike();
                break;
            }
        pick = new int[2] { -1, -1 };
        StartCoroutine(Deal(false));
    }

    private IEnumerator Deal(bool o)
    {
        float e = 0;
        Vector2 pos = new Vector2(0, 0);
        float a = 0;
        if (o)
        {
            Audio.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.WireSequenceMechanism, transform);
            while (e < 1)
            {
                e += Time.deltaTime * 2;
                pos = new Vector2(Mathf.Lerp(0.02315f, 0.015f, e), Mathf.Lerp(0.0628f, 0.0316f, e));
                objects[12].localPosition = new Vector3(-0.05279f, pos.x, pos.y);
                objects[13].localPosition = new Vector3(0.05279f, pos.x, pos.y);
                a = Mathf.Lerp(4.245f, 18f, e);
                objects[12].localEulerAngles = new Vector3(a, 180, 0);
                objects[13].localEulerAngles = new Vector3(a, 180, 0);
                yield return null;
            }
            objects[12].localPosition = new Vector3(-0.05279f, 0.015f, 0.0316f);
            objects[13].localPosition = new Vector3(0.05279f, 0.015f, 0.0316f);
            objects[12].localEulerAngles = new Vector3(18, 180, 0);
            objects[13].localEulerAngles = new Vector3(18, 180, 0);
            e = 0;
            while (e < 1)
            {
                e += Time.deltaTime * 2;
                objects[14].localPosition = new Vector3(-0.0528f, Mathf.Lerp(0.0171f, 0.0293f, e), 0.0615f);
                objects[15].localPosition = new Vector3(0.0528f, Mathf.Lerp(0.0171f, 0.0293f, e), 0.0615f);
                for (int i = 0; i < 12; i++)
                    objects[i].localPosition = new Vector3(-0.0526f, Mathf.Lerp(0.0124f, 0.02434f, e) + (i * 0.00022f), 0.0902f);
                yield return null;
            }
            objects[14].localPosition = new Vector3(-0.0528f, 0.0293f, 0.0615f);
            objects[15].localPosition = new Vector3(0.0528f, 0.0293f, 0.0615f);
            for (int i = 0; i < 12; i++)
                objects[i].localPosition = new Vector3(-0.0526f, 0.02434f + (i * 0.0013f), 0.0902f);
            float x = 0;
            float z = 0;
            Audio.PlaySoundAtTransform("Deal", transform);
            for (int i = 11; i >= 0; i--)
            {
                x = new float[] { -0.034704f, -0.011504f, 0.011696f, 0.034896f}[i % 4];
                z = new float[] { 0.0369f, 0.0007f, -0.0358f}[i / 4];
                StartCoroutine(DealCard(i, new Vector3(x, 0.0182f, z), true));
                yield return new WaitForSeconds(0.033f);
            }
            yield return new WaitForSeconds(0.33f);
            pressable = true;
        }
        if (!o)
        {
            pressable = false;
            float y = 0;
            Audio.PlaySoundAtTransform("Deal", transform);
            for (int i = 0; i < 12; i++)
            {
                y = 0.02434f + (i * 0.00022f);
                StartCoroutine(DealCard(i, new Vector3(0.053f, y, 0.0902f), false));
                yield return new WaitForSeconds(0.033f);
            }
            yield return new WaitForSeconds(0.33f);
            e = 1;
            Audio.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.WireSequenceMechanism, transform);
            while (e > 0)
            {
                e -= Time.deltaTime * 2;
                objects[14].localPosition = new Vector3(-0.0528f, Mathf.Lerp(0.0171f, 0.0293f, e), 0.0615f);
                objects[15].localPosition = new Vector3(0.0528f, Mathf.Lerp(0.0171f, 0.0293f, e), 0.0615f);
                for (int i = 0; i < 12; i++)
                    objects[i].localPosition = new Vector3(0.0526f, Mathf.Lerp(0.0121f, 0.02434f, e) + (i * 0.00022f), 0.0902f);
                yield return null;
            }
            objects[14].localPosition = new Vector3(-0.0528f, 0.0171f, 0.0615f);
            objects[15].localPosition = new Vector3(0.0528f, 0.0171f, 0.0615f);
            e = 1;
            while (e > 0)
            {
                e -= Time.deltaTime * 2;
                pos = new Vector2(Mathf.Lerp(0.02315f, 0.015f, e), Mathf.Lerp(0.0628f, 0.0316f, e));
                objects[12].localPosition = new Vector3(-0.05279f, pos.x, pos.y);
                objects[13].localPosition = new Vector3(0.05279f, pos.x, pos.y);
                a = Mathf.Lerp(4.245f, 18f, e);
                objects[12].localEulerAngles = new Vector3(a, 180, 0);
                objects[13].localEulerAngles = new Vector3(a, 180, 0);
                yield return null;
            }
            objects[12].localEulerAngles = new Vector3(4.245f, 180, 0);
            objects[13].localEulerAngles = new Vector3(4.245f, 180, 0);
            pressable = false;
        }
    }

    private IEnumerator DealCard(int c, Vector3 v, bool d)
    {
        float e = 0;
        float[] limarg = new float[2];
        limarg[1] = d ? 0 : 90;
        limarg[0] = 90 + limarg[1];
        float flip = (match[c] || pick.Contains(c)) ? 0 : 180;
        Vector3 u = objects[c].localPosition;
        while (e < 1)
        {
            e += Time.deltaTime * 3;
            float y = d ? e * e : Mathf.Sqrt(e);
            objects[c].localPosition = new Vector3(Mathf.Lerp(u.x, v.x, e), Mathf.Lerp(u.y, v.y, y), Mathf.Lerp(u.z, v.z, e));
            objects[c].localEulerAngles = new Vector3(0, Mathf.Lerp(limarg[0], limarg[1], e), flip);
            yield return null;
        }
        objects[c].localPosition = v;
        objects[c].localEulerAngles = new Vector3(0, limarg[1], flip);
    }

    private IEnumerator Flip(int c, bool check)
    {
        pressable = false;
        if (check)
        {
            if (pick[0] < 0)
                pick[0] = c;
            else
                pick[1] = c;
        }
        float e = 0;
        objects[c].localPosition += new Vector3(0, 0.0069f, 0);
        while(e < 1)
        {
            e += Time.deltaTime * 7;
            objects[c].localEulerAngles = new Vector3(0, 0, 180 * (e + (check ? 1 : 0)));
            yield return null;
        }
        objects[c].localEulerAngles = new Vector3(0, 0, 180 * (check ? 0 : 1));
        objects[c].localPosition -= new Vector3(0, 0.0069f, 0);
        if (check && pick[1] >= 0)
        {
            if (arr[pick[0]] == arr[pick[1]])
            {               
                match[pick[0]] = true;
                match[pick[1]] = true;
                if (match.All(x => x))
                {
                    Debug.LogFormat("[Memory Match #{0}] All pairs matched.", moduleID);
                    Audio.PlaySoundAtTransform("Allmatch", transform);
                }
                else
                    Audio.PlaySoundAtTransform("Match", transform);
                pressable = true;
            }
            else
            {
                yield return new WaitForSeconds(0.35f);
                Audio.PlaySoundAtTransform("Flip", objects[c]);
                StartCoroutine(Flip(pick[0], false));
                StartCoroutine(Flip(pick[1], false));
            }
            pick = new int[2] { -1, -1 };
        }
        else
            pressable = true;
    }
}
