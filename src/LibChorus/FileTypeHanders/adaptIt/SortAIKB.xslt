<?xml version="1.0" encoding="UTF-8" ?>
<xsl:stylesheet version="1.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform">

  <!--have a template to copy all the attributes (so we don't have to update if
	  attributes are added-->
  <xsl:template match="@*">
	<xsl:copy>
	  <xsl:apply-templates select="@*"/>
	</xsl:copy>
  </xsl:template>

  <xsl:template match="/KB">
	<KB>
	  <xsl:apply-templates select="@*"/>
	  <xsl:for-each select="MAP">
		<xsl:sort select="@mn" order="ascending"/>
		<MAP>
		  <xsl:apply-templates select="@*"/>
		  <xsl:for-each select="TU">
			<xsl:sort select="@k" order="ascending"/>
			<TU>
			  <xsl:apply-templates select="@*"/>
			  <xsl:for-each select="RS">
				<RS>
				  <xsl:apply-templates select="@*"/>
				</RS>
			  </xsl:for-each>
			</TU>
		  </xsl:for-each>
		</MAP>
	  </xsl:for-each>
	</KB>
  </xsl:template>
</xsl:stylesheet>
