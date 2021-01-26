/* From http://xmlunit.sourceforge.net/ Moved here because the original library is for testing, and
 * it is tied to nunit, which we don't want to ship in production
 */

using System;
using System.IO;
using System.Xml;
using Chorus.merge.xml.generic.xmldiff;

namespace Chorus.merge.xml.generic.xmldiff
{
	public class XmlDiff
	{
		private bool _continueComparing;
		private readonly XmlReader _controlReader;
		private readonly XmlReader _testReader;
		private readonly DiffConfiguration _diffConfiguration;
		private DiffResult _diffResult;

		public XmlDiff(XmlInput control, XmlInput test,
					   DiffConfiguration diffConfiguration)
		{
			_diffConfiguration = diffConfiguration;
			_controlReader = CreateXmlReader(control);
			if (control.Equals(test))
			{
				_testReader = _controlReader;
			}
			else
			{
				_testReader = CreateXmlReader(test);
			}
		}

		public XmlDiff(XmlInput control, XmlInput test)
			: this(control, test, new DiffConfiguration())
		{
		}

		public XmlDiff(TextReader control, TextReader test)
			: this(new XmlInput(control), new XmlInput(test))
		{
		}

		public XmlDiff(string control, string test)
			: this(new XmlInput(control), new XmlInput(test))
		{
		}

		private XmlReader CreateXmlReader(XmlInput forInput)
		{
			XmlReader xmlReader = forInput.CreateXmlReader();

			if (xmlReader is XmlTextReader)
			{
				((XmlTextReader)xmlReader).WhitespaceHandling = _diffConfiguration.WhitespaceHandling;
			}

			if (_diffConfiguration.UseValidatingParser)
			{
#pragma warning disable 612,618
				XmlValidatingReader validatingReader = new XmlValidatingReader(xmlReader);
#pragma warning restore 612,618
				return validatingReader;
			}

			return xmlReader;
		}

		public DiffResult Compare()
		{
			if (_diffResult == null)
			{
				_diffResult = new DiffResult();
				if (!_controlReader.Equals(_testReader))
				{
					try
					{
						Compare(_diffResult);
					}
					catch(Exception e)
					{
						throw e;//just need a place to put a breakpoint
					}
				}
			}
			return _diffResult;
		}

		private void Compare(DiffResult result)
		{
			_continueComparing = true;
			bool controlRead, testRead;
			// Yuck! (Says RandyR) Exceptions are too expensive to use to control program flow like this.
			// Compare these times with and without the exceptions:
			// ZPI data set, DM09->DM10
			// With 19K exceptions: RevisionInspector.GetChangeRecords: 00:02:31.5675604
			// Use bool (_continueComparing) to control it, not exception: RevisionInspector.GetChangeRecords: 00:00:30.9605999
			// The bool code is two minutes faster.
			//try
			//{
				do
				{
					controlRead = _controlReader.Read();
					try
					{
						testRead = _testReader.Read();
					}
					catch(Exception e)
					{
						throw e;//just need a place to put a breakpoint
					}
					Compare(result, ref controlRead, ref testRead);
				} while (_continueComparing && controlRead && testRead);
			//}
			//catch (FlowControlException e)
			//{
			//    //what is this? it's how this class stops looking for more differences,
			//    //by throwing this exception, making us jump back up here.
			//    //Console.Out.WriteLine(e.Message);
			//}
		}

		private void Compare(DiffResult result, ref bool controlRead, ref bool testRead)
		{
			if (controlRead)
			{
				if (testRead)
				{
					CompareNodes(result);
					CheckEmptyOrAtEndElement(result, ref controlRead, ref testRead);
				}
				else
				{
					if (_testReader.NodeType == XmlNodeType.None && _controlReader.NodeType == XmlNodeType.EndElement)
					{
						DifferenceFound(DifferenceType.EMPTY_NODE_ID, result);
					}
					else
					{
						DifferenceFound(DifferenceType.CHILD_NODELIST_LENGTH_ID, result);
					}
					return;
				}
			}
			//jh added this; under a condition I haven't got into an xdiff test yet, the
			// 'test' guy still had more children, and this fact was being missed by the above code
			// I (RBR) discovered the context in which this happens. it is:
			// Control: <Run />
			// Test:    <Run></Run>
			// At this point Control is at node type 'none', while test is at EndElement.
			if (controlRead != testRead)
			{
				if (_controlReader.NodeType == XmlNodeType.None && _testReader.NodeType == XmlNodeType.EndElement)
				{
					DifferenceFound(DifferenceType.EMPTY_NODE_ID, result);
				}
				else
				{
					DifferenceFound(DifferenceType.CHILD_NODELIST_LENGTH_ID, result);
				}
			}
		}

		private void CompareNodes(DiffResult result)
		{
			XmlNodeType controlNodeType = _controlReader.NodeType;
			XmlNodeType testNodeType = _testReader.NodeType;
			if (!controlNodeType.Equals(testNodeType))
			{
				CheckNodeTypes(controlNodeType, testNodeType, result);
			}
			else if (controlNodeType == XmlNodeType.Element)
			{
				CompareElements(result);
			}
			else if (controlNodeType == XmlNodeType.Text)
			{
				CompareText(result, DifferenceType.TEXT_VALUE_ID);
			}
			else if (controlNodeType == XmlNodeType.CDATA)
			{
				CompareText(result, DifferenceType.CDATA_VALUE_ID);
			}
		}

		private void CheckNodeTypes(XmlNodeType controlNodeType, XmlNodeType testNodeType, DiffResult result)
		{
			XmlReader readerToAdvance = null;
			if (controlNodeType.Equals(XmlNodeType.XmlDeclaration))
			{
				readerToAdvance = _controlReader;
			}
			else if (testNodeType.Equals(XmlNodeType.XmlDeclaration))
			{
				readerToAdvance = _testReader;
			}

			if (readerToAdvance != null)
			{
				DifferenceFound(DifferenceType.HAS_XML_DECLARATION_PREFIX_ID,
								controlNodeType, testNodeType, result);
				readerToAdvance.Read();
				CompareNodes(result);
			}
			else
			{
				DifferenceFound(DifferenceType.NODE_TYPE_ID, controlNodeType,
								testNodeType, result);
			}
		}

		private void CompareElements(DiffResult result)
		{
			string controlTagName = _controlReader.Name;
			string testTagName = _testReader.Name;
			if (!String.Equals(controlTagName, testTagName))
			{
				DifferenceFound(DifferenceType.ELEMENT_TAG_NAME_ID, result);
			}
			else
			{
				int controlAttributeCount = _controlReader.AttributeCount;
				int testAttributeCount = _testReader.AttributeCount;
				if (controlAttributeCount != testAttributeCount)
				{
					DifferenceFound(DifferenceType.ELEMENT_NUM_ATTRIBUTES_ID, result);
				}
				else
				{
					CompareAttributes(result, controlAttributeCount);
				}
			}
		}

		private void CompareAttributes(DiffResult result, int controlAttributeCount)
		{
			string controlAttrValue, controlAttrName;
			string testAttrValue, testAttrName;

			var movedToControlAttr = _controlReader.MoveToFirstAttribute();
			var movedToTestAttr = _testReader.MoveToFirstAttribute();
			for (int i = 0; _continueComparing && i < controlAttributeCount; ++i)
			{

				controlAttrName = _controlReader.Name;
				testAttrName = _testReader.Name;

				controlAttrValue = _controlReader.Value;
				testAttrValue = _testReader.Value;



					if (!String.Equals(controlAttrName, testAttrName))
					{
						DifferenceFound(DifferenceType.ATTR_SEQUENCE_ID, result);

						if (!_testReader.MoveToAttribute(controlAttrName))
						{
							DifferenceFound(DifferenceType.ATTR_NAME_NOT_FOUND_ID, result);
						}
						testAttrValue = _testReader.Value;
					}

				//Hatton hack for LIFT: this is just not enough reason to tell the user there  was a change,
				//since it's basically a bug in the LIFT edittor, if it's the only change, and this diff
				//framework doesn't report *what* changed, so we can't filter it out later.
				if (!string.Equals(controlAttrName, "dateModified"))
				{
				   if (!String.Equals(controlAttrValue, testAttrValue))
					{
						DifferenceFound(DifferenceType.ATTR_VALUE_ID, result);
					}
				}
				_controlReader.MoveToNextAttribute();
				_testReader.MoveToNextAttribute();
			}
			if (movedToControlAttr)
				_controlReader.MoveToElement();
			if (movedToTestAttr)
				_testReader.MoveToElement();
		}

		private void CompareText(DiffResult result, DifferenceType type)
		{
			string controlText = _controlReader.Value;
			string testText = _testReader.Value;
			if (!string.Equals(controlText, testText))
			{
				DifferenceFound(type, result);
			}
		}

		private void DifferenceFound(DifferenceType differenceType, DiffResult result)
		{
			DifferenceFound(new Difference(differenceType), result);
		}

		private void DifferenceFound(Difference difference, DiffResult result)
		{
			result.DifferenceFound(this, difference);
			if (!ContinueComparison(difference))
			{
				// Don't even think of using exceptions to control program flow. They are too expensive!
				//throw new FlowControlException(difference);
				_continueComparing = false;
			}
		}

		private void DifferenceFound(DifferenceType differenceType,
									 XmlNodeType controlNodeType,
									 XmlNodeType testNodeType,
									 DiffResult result)
		{
			DifferenceFound(new Difference(differenceType, controlNodeType, testNodeType),
							result);
		}

		private bool ContinueComparison(Difference afterDifference)
		{
			return !afterDifference.MajorDifference;
		}

		private void CheckEmptyOrAtEndElement(DiffResult result,
											  ref bool controlRead, ref bool testRead)
		{
			if (_controlReader.IsEmptyElement)
			{
				if (!_testReader.IsEmptyElement)
				{
					CheckEndElement(_testReader, ref testRead, result);
				}
			}
			else
			{
				if (_testReader.IsEmptyElement)
				{
					CheckEndElement(_controlReader, ref controlRead, result);
				}
			}
		}

		private void CheckEndElement(XmlReader reader, ref bool readResult, DiffResult result)
		{
			readResult = reader.Read();
			if (!readResult || reader.NodeType != XmlNodeType.EndElement)
			{
				DifferenceFound(
					reader.NodeType == XmlNodeType.Text
						? DifferenceType.TEXT_VALUE_ID
						: DifferenceType.CHILD_NODELIST_LENGTH_ID, result);
			}
		}

		public string OptionalDescription
		{
			get
			{
				return _diffConfiguration.Description;
			}
		}
	}
}