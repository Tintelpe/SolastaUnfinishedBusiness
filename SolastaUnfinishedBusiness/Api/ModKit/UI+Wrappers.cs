﻿// Copyright < 2021 > Narria (github user Cabarius) - License: MIT

using UnityEngine;
using GL = UnityEngine.GUILayout;

namespace SolastaUnfinishedBusiness.Api.ModKit;

internal static partial class UI
{
    // GUILayout wrappers and extensions so other modules can use UI.MethodName()
    public static GUILayoutOption ExpandWidth(bool v)
    {
        return GL.ExpandWidth(v);
    }

    public static GUILayoutOption ExpandHeight(bool v)
    {
        return GL.ExpandHeight(v);
    }

    public static GUILayoutOption AutoWidth()
    {
        return GL.ExpandWidth(false);
    }

    public static GUILayoutOption AutoHeight()
    {
        return GL.ExpandHeight(false);
    }

    public static GUILayoutOption Width(float v)
    {
        return GL.Width(v);
    }

    public static GUILayoutOption width(this int v)
    {
        return GL.Width(v);
    }

    public static GUILayoutOption[] Width(float min, float max)
    {
        return new[] { GL.MinWidth(min), GL.MaxWidth(max) };
    }

    public static GUILayoutOption[] Height(float min, float max)
    {
        return new[] { GL.MinHeight(min), GL.MaxHeight(max) };
    }

    public static GUILayoutOption Height(float v)
    {
        return GL.Height(v);
    }

    public static GUILayoutOption MaxWidth(float v)
    {
        return GL.MaxWidth(v);
    }

    public static GUILayoutOption MaxHeight(float v)
    {
        return GL.MaxHeight(v);
    }

    public static GUILayoutOption MinWidth(float v)
    {
        return GL.MinWidth(v);
    }

    public static GUILayoutOption MinHeight(float v)
    {
        return GL.MinHeight(v);
    }

    public static void Space(float size = 150f)
    {
        GL.Space(size);
    }

    public static void space(this int size)
    {
        GL.Space(size);
    }

    public static void Indent(int indent, float size = 75f)
    {
        GL.Space(indent * size);
    }

    public static void BeginHorizontal(GUIStyle style, params GUILayoutOption[] options)
    {
        GL.BeginHorizontal(style, options);
    }

    public static void BeginHorizontal(params GUILayoutOption[] options)
    {
        GL.BeginHorizontal(options);
    }

    public static void EndHorizontal()
    {
        GL.EndHorizontal();
    }

    public static GUILayout.AreaScope AreaScope(Rect screenRect)
    {
        return new(screenRect);
    }

    public static GUILayout.AreaScope AreaScope(Rect screenRect, string text)
    {
        return new(screenRect, text);
    }

    public static GUILayout.HorizontalScope HorizontalScope(params GUILayoutOption[] options)
    {
        return new(options);
    }

    public static GUILayout.HorizontalScope HorizontalScope(float width)
    {
        return new(Width(width));
    }

    public static GUILayout.HorizontalScope HorizontalScope(GUIStyle style, params GUILayoutOption[] options)
    {
        return new(style, options);
    }

    public static GUILayout.HorizontalScope HorizontalScope(GUIStyle style, float width)
    {
        return new(style, Width(width));
    }

    public static GUILayout.VerticalScope VerticalScope(params GUILayoutOption[] options)
    {
        return new(options);
    }

    public static GUILayout.VerticalScope VerticalScope(GUIStyle style, params GUILayoutOption[] options)
    {
        return new(style, options);
    }

    public static GUILayout.ScrollViewScope ScrollViewScope(Vector2 scrollPosition, params GUILayoutOption[] options)
    {
        return new(scrollPosition, options);
    }

    public static GUILayout.ScrollViewScope ScrollViewScope(Vector2 scrollPosition, GUIStyle style,
        params GUILayoutOption[] options)
    {
        return new(scrollPosition, style, options);
    }

    public static void BeginVertical(params GUILayoutOption[] options)
    {
        GL.BeginVertical(options);
    }

    public static void BeginVertical(GUIStyle style, params GUILayoutOption[] options)
    {
        GL.BeginVertical(style, options);
    }

    public static void EndVertical()
    {
        GL.EndVertical();
    }
}
