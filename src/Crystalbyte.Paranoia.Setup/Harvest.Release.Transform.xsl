<?xml version="1.0" encoding="UTF-8"?>
<xsl:stylesheet version="2.0"
    xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
    xmlns="http://schemas.microsoft.com/wix/2006/wi"
    xmlns:wix="http://schemas.microsoft.com/wix/2006/wi"
    xmlns:msxsl="urn:schemas-microsoft-com:xslt" exclude-result-prefixes="msxsl">

  <!-- Copy all attributes and elements to the output. -->
  <xsl:template match="@*|*">
    <xsl:copy>
      <xsl:apply-templates select="@*" />
      <xsl:apply-templates select="*" />
    </xsl:copy>
  </xsl:template>

  <xsl:output method="xml" indent="yes" />

  <!-- Create searches for the directories to remove. -->
  <xsl:key name="xml-search" match="wix:Component[contains(wix:File/@Source, '.xml')]" use="@Id"/>
  <xsl:key name="pdb-search" match="wix:Component[contains(wix:File/@Source, '.pdb')]" use="@Id"/>

  <!-- Remove matching components-->
  <xsl:template match="wix:Component[key('xml-search', @Id)]" />
  <xsl:template match="wix:Component[key('pdb-search', @Id)]" />

</xsl:stylesheet>