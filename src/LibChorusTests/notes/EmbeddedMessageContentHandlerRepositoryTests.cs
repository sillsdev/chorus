// Copyright (c) 2015 SIL International
// This software is licensed under the MIT License (http://opensource.org/licenses/MIT)
using System;
using System.Linq;
using NUnit.Framework;
using Chorus.notes;

namespace LibChorus.Tests.notes
{
	[TestFixture]
	public class EmbeddedMessageContentHandlerRepositoryTests
	{
		[Test]
		public void KnownHandlers_ReturnsDefaultLast()
		{
			Assert.That(new EmbeddedMessageContentHandlerRepository().KnownHandlers.Last().GetType(),
				Is.EqualTo(typeof(DefaultEmbeddedMessageContentHandler)));
		}

		[Test]
		public void KnownHandlers_ContainsHandlersFromChorusExe()
		{
			Assert.That(new EmbeddedMessageContentHandlerRepository().KnownHandlers.Select(x => x.GetType().Name),
				Has.Member("MergeConflictEmbeddedMessageContentHandler"));
		}
	}
}

