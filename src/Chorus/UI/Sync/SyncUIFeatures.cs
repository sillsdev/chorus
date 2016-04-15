// Copyright (c) 2016 SIL International
// This software is licensed under the MIT License (http://opensource.org/licenses/MIT)

using System;

namespace Chorus.UI.Sync
{
	[Flags]
	public enum SyncUIFeatures
	{
		Minimal =0,
		SendReceiveButton=2,
		TaskList=4,
		Log = 8,
		SimpleRepositoryChooserInsteadOfAdvanced = 16,
		PlaySoundIfSuccessful = 32,
		NormalRecommended = 0xFFFF - (SendReceiveButton),
		Advanced = 0xFFFF - (SimpleRepositoryChooserInsteadOfAdvanced)
	}
}