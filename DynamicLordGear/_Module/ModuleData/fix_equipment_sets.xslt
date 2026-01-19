<xsl:stylesheet version="1.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform">
	<xsl:output omit-xml-declaration="yes"/>
	<xsl:template match="@*|node()">
		<xsl:copy>
			<xsl:apply-templates select="@*|node()"/>
		</xsl:copy>
	</xsl:template>

	<xsl:template match="EquipmentRoster[@id='emp_bat_template_medium']/Flags">
		<xsl:copy>
			<!-- Copy existing attributes -->
			<xsl:apply-templates select="@*"/>
			<!-- Add the new attribute -->
			<xsl:attribute name="IsCombatantTemplate">true</xsl:attribute>
			<!-- Process child nodes -->
			<xsl:apply-templates select="node()"/>
		</xsl:copy>
	</xsl:template>

	<xsl:template match="EquipmentRoster[@id='nord_king_template_civ_f']/Flags">
		<xsl:copy>
			<!-- Copy existing attributes -->
			<xsl:apply-templates select="@*"/>
			<!-- Add the new attribute -->
			<xsl:attribute name="IsFemaleTemplate">true</xsl:attribute>
			<!-- Process child nodes -->
			<xsl:apply-templates select="node()"/>
		</xsl:copy>
	</xsl:template>
</xsl:stylesheet>