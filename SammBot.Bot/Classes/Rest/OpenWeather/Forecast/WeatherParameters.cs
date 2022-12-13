﻿#region License Information (GPLv3)
/*
 * Samm-Bot - A lightweight Discord.NET bot for moderation and other purposes.
 * Copyright (C) 2021-2022 AestheticalZ
 *
 *  This program is free software: you can redistribute it and/or modify
 *  it under the terms of the GNU General Public License as published by
 *  the Free Software Foundation, either version 3 of the License, or
 *  (at your option) any later version.
 *
 *  This program is distributed in the hope that it will be useful,
 *  but WITHOUT ANY WARRANTY; without even the implied warranty of
 *  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 *  GNU General Public License for more details.
 *
 *  You should have received a copy of the GNU General Public License
 *  along with this program.  If not, see <https://www.gnu.org/licenses/>.
 */
#endregion

using SammBot.Bot.Attributes;

namespace SammBot.Bot.Rest.OpenWeather.Forecast;

public class WeatherParameters
{
    [UglyName("lat")]
    public float Latitude { get; set; }
    [UglyName("lon")]
    public float Longitude { get; set; }
    [UglyName("appid")]
    public string AppId { get; set; }
    [UglyName("units")]
    public string Units { get; set; }
}