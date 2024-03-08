﻿/*
** File Name: SAM_SEConverters.cs
** Author: s_eva
** Creation date: January 2024
** Description: This file regroups all the converters for XAML Files.
** Licence: LGPLv3
** Copyright (c) 2023-Present Synchronic
*/

using System.Globalization;
using System.Windows.Data;
using System.Windows;

namespace Leosac.KeyManager.Library.KeyStore.SAM_SE.UI.Domain
{
    public class BooleanToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            bool boolValue = (bool)value;
            boolValue = (parameter != null) ? !boolValue : boolValue;
            return boolValue ? Visibility.Visible : Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}