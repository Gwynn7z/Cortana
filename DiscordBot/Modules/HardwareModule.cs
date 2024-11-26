﻿using Discord;
using Discord.Interactions;
using Processor;

namespace DiscordBot.Modules;

[Group("hardware", "Gestione domotica")]
[RequireOwner]
public class HardwareModule : InteractionModuleBase<SocketInteractionContext>
{
	[SlashCommand("lamp", "Accendi o spegni la luce")]
	public async Task LightToggle()
	{
		string result = Hardware.PowerLamp(ETrigger.Toggle);
		Embed embed = DiscordUtils.CreateEmbed(result);
		await RespondAsync(embed: embed, ephemeral: true);
	}

	[SlashCommand("hardware", "Interagisci con i dispositivi hardware", runMode: RunMode.Async)]
	public async Task HardwareInteract([Summary("dispositivo", "Con cosa vuoi interagire?")] EDevice element, [Summary("azione", "Cosa vuoi fare?")] ETrigger trigger)
	{
		await DeferAsync(true);

		string result = Hardware.SwitchFromEnum(element, trigger);

		Embed embed = DiscordUtils.CreateEmbed(result);
		await FollowupAsync(embed: embed, ephemeral: true);
	}

	[SlashCommand("info", "Ricevi informazioni sull'hardware", runMode: RunMode.Async)]
	public async Task HardwareInfo([Summary("info", "Quale informazione vuoi?")] EHardwareInfo info)
	{
		await DeferAsync(true);

		string result = info switch
		{
			EHardwareInfo.Location => Hardware.GetLocation(),
			EHardwareInfo.Ip => await Hardware.GetPublicIp(),
			EHardwareInfo.Gateway => Hardware.GetDefaultGateway(),
			EHardwareInfo.Temperature => Hardware.GetCpuTemperature(),
			_ => "Informazione non disponibile"
		};

		Embed embed = DiscordUtils.CreateEmbed(result);
		await FollowupAsync(embed: embed, ephemeral: true);
	}
}