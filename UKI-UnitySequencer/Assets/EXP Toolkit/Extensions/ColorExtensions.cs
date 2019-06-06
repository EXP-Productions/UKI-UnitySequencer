using UnityEngine;
using System.Collections;

public static class ColorExtensions
{
    public static Color BlackTransparent = new Color(0, 0, 0, 0);

	public static Color SetColourWithoutAlpha( this Color col, Color newCol )
	{
		col.r = newCol.r;
		col.g = newCol.g;
		col.b = newCol.b;

		return col;
	}

	public static Color SetAlpha( this Color col, float alpha )
	{
		col.a = alpha;
		return col;
	}

	public static Color SetHue( this Color col, float val )
	{
		HSBColor temphsbCol = HSBColor.FromColor( col );
		temphsbCol.h = val;

		return temphsbCol.ToColor();
	}

	public static Color SetSaturation( this Color col, float val )
	{
		HSBColor temphsbCol = HSBColor.FromColor( col );
		temphsbCol.s = val;
		
		return temphsbCol.ToColor();
	}

	public static Color SetBright( this Color col, float val )
	{
		HSBColor temphsbCol = HSBColor.FromColor( col );
		temphsbCol.b = val;
		
		return temphsbCol.ToColor();
	}

	public static float GetHue( this Color col, float val )
	{
		HSBColor temphsbCol = HSBColor.FromColor( col );
		return temphsbCol.h;
	}
	
	public static float GetSaturation( this Color col, float val )
	{
		HSBColor temphsbCol = HSBColor.FromColor( col );
		return temphsbCol.s;
	}
	
	public static float GetBright( this Color col, float val )
	{
		HSBColor temphsbCol = HSBColor.FromColor( col );
		return temphsbCol.b;
	}

    public static Color Clamp01(this Color col)
    {
        col.r = Mathf.Clamp01(col.r);
        col.g = Mathf.Clamp01(col.g);
        col.b = Mathf.Clamp01(col.b);

        col.a = Mathf.Clamp01(col.a);

        return col;
    }

    public static Color SetContrastCoefficient(this Color col, float power, bool includeAlpha )
    {
        col.r = Mathf.Pow(col.r, power);
        col.g = Mathf.Pow(col.g, power);
        col.b = Mathf.Pow(col.b, power);

        if (includeAlpha )
            col.a = Mathf.Pow(col.a, power);

        return col;
    }


	public static Color LerpTriple( Color col1, Color col2, Color col3, float value )
	{
		if( value < .5f )
		{
			return Color.Lerp( col1, col2, value * 2 );
		}
		else
		{
			return Color.Lerp( col2, col3, (value - .5f) * 2 );
		}
	}

    
    public static Color Parse(string rString)
    {
        string[] temp = rString.Substring(1, rString.Length - 2).Split(',');
        float r = float.Parse(temp[0]);
        float g = float.Parse(temp[1]);
        float b = float.Parse(temp[2]);
        float a = float.Parse(temp[2]);
        Color rValue = new Color(r,g,b,a);
        return rValue;
    }


    public static string ToString(Color c)
    {
        float r = c.r;
        float g = c.g;
        float b = c.b;
        float a = c.a;
        string rValue = r.ToDoubleDecimalString() + "," + g.ToDoubleDecimalString() + "," + b.ToDoubleDecimalString() + "," + a.ToDoubleDecimalString();
        return rValue;
    }
}
