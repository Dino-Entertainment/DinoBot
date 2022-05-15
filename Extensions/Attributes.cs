﻿using System;

namespace SammBotNET.Extensions
{
	[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
	public class HideInHelp : Attribute
	{
		public HideInHelp() { }
	}

	[AttributeUsage(AttributeTargets.Class)]
	public class ModuleEmoji : Attribute
	{
		public string Emoji = string.Empty;

		public ModuleEmoji(string Emoji) => this.Emoji = Emoji;
	}

	[AttributeUsage(AttributeTargets.Class)]
	public class RegisterHook : Attribute
	{
		public RegisterHook() { }
	}

	[AttributeUsage(AttributeTargets.Property)]
	public class NotModifiable : Attribute
	{
		public NotModifiable() { }
	}
}
