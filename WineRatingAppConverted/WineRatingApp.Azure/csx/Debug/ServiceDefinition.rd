<?xml version="1.0" encoding="utf-8"?>
<serviceModel xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance" xmlns:xsd="http://www.w3.org/2001/XMLSchema" name="WineRatingApp.Azure" generation="1" functional="0" release="0" Id="c8b5c5c7-b226-43a9-be13-f371147aab92" dslVersion="1.2.0.0" xmlns="http://schemas.microsoft.com/dsltools/RDSM">
  <groups>
    <group name="WineRatingApp.AzureGroup" generation="1" functional="0" release="0">
      <componentports>
        <inPort name="WineRatingApp:Endpoint1" protocol="http">
          <inToChannel>
            <lBChannelMoniker name="/WineRatingApp.Azure/WineRatingApp.AzureGroup/LB:WineRatingApp:Endpoint1" />
          </inToChannel>
        </inPort>
      </componentports>
      <settings>
        <aCS name="WineRatingApp:Microsoft.WindowsAzure.Plugins.Diagnostics.ConnectionString" defaultValue="">
          <maps>
            <mapMoniker name="/WineRatingApp.Azure/WineRatingApp.AzureGroup/MapWineRatingApp:Microsoft.WindowsAzure.Plugins.Diagnostics.ConnectionString" />
          </maps>
        </aCS>
        <aCS name="WineRatingAppInstances" defaultValue="[1,1,1]">
          <maps>
            <mapMoniker name="/WineRatingApp.Azure/WineRatingApp.AzureGroup/MapWineRatingAppInstances" />
          </maps>
        </aCS>
      </settings>
      <channels>
        <lBChannel name="LB:WineRatingApp:Endpoint1">
          <toPorts>
            <inPortMoniker name="/WineRatingApp.Azure/WineRatingApp.AzureGroup/WineRatingApp/Endpoint1" />
          </toPorts>
        </lBChannel>
      </channels>
      <maps>
        <map name="MapWineRatingApp:Microsoft.WindowsAzure.Plugins.Diagnostics.ConnectionString" kind="Identity">
          <setting>
            <aCSMoniker name="/WineRatingApp.Azure/WineRatingApp.AzureGroup/WineRatingApp/Microsoft.WindowsAzure.Plugins.Diagnostics.ConnectionString" />
          </setting>
        </map>
        <map name="MapWineRatingAppInstances" kind="Identity">
          <setting>
            <sCSPolicyIDMoniker name="/WineRatingApp.Azure/WineRatingApp.AzureGroup/WineRatingAppInstances" />
          </setting>
        </map>
      </maps>
      <components>
        <groupHascomponents>
          <role name="WineRatingApp" generation="1" functional="0" release="0" software="C:\dev\wine\WineRatingAppConverted\WineRatingApp.Azure\csx\Debug\roles\WineRatingApp" entryPoint="base\x64\WaHostBootstrapper.exe" parameters="base\x64\WaIISHost.exe " memIndex="-1" hostingEnvironment="frontendadmin" hostingEnvironmentVersion="2">
            <componentports>
              <inPort name="Endpoint1" protocol="http" portRanges="80" />
            </componentports>
            <settings>
              <aCS name="Microsoft.WindowsAzure.Plugins.Diagnostics.ConnectionString" defaultValue="" />
              <aCS name="__ModelData" defaultValue="&lt;m role=&quot;WineRatingApp&quot; xmlns=&quot;urn:azure:m:v1&quot;&gt;&lt;r name=&quot;WineRatingApp&quot;&gt;&lt;e name=&quot;Endpoint1&quot; /&gt;&lt;/r&gt;&lt;/m&gt;" />
            </settings>
            <resourcereferences>
              <resourceReference name="DiagnosticStore" defaultAmount="[4096,4096,4096]" defaultSticky="true" kind="Directory" />
              <resourceReference name="EventStore" defaultAmount="[1000,1000,1000]" defaultSticky="false" kind="LogStore" />
            </resourcereferences>
          </role>
          <sCSPolicy>
            <sCSPolicyIDMoniker name="/WineRatingApp.Azure/WineRatingApp.AzureGroup/WineRatingAppInstances" />
            <sCSPolicyUpdateDomainMoniker name="/WineRatingApp.Azure/WineRatingApp.AzureGroup/WineRatingAppUpgradeDomains" />
            <sCSPolicyFaultDomainMoniker name="/WineRatingApp.Azure/WineRatingApp.AzureGroup/WineRatingAppFaultDomains" />
          </sCSPolicy>
        </groupHascomponents>
      </components>
      <sCSPolicy>
        <sCSPolicyUpdateDomain name="WineRatingAppUpgradeDomains" defaultPolicy="[5,5,5]" />
        <sCSPolicyFaultDomain name="WineRatingAppFaultDomains" defaultPolicy="[2,2,2]" />
        <sCSPolicyID name="WineRatingAppInstances" defaultPolicy="[1,1,1]" />
      </sCSPolicy>
    </group>
  </groups>
  <implements>
    <implementation Id="795023e2-7105-4f3f-9d52-a49374e37842" ref="Microsoft.RedDog.Contract\ServiceContract\WineRatingApp.AzureContract@ServiceDefinition">
      <interfacereferences>
        <interfaceReference Id="9521519c-d12e-43f4-b3c1-879f9b3eff95" ref="Microsoft.RedDog.Contract\Interface\WineRatingApp:Endpoint1@ServiceDefinition">
          <inPort>
            <inPortMoniker name="/WineRatingApp.Azure/WineRatingApp.AzureGroup/WineRatingApp:Endpoint1" />
          </inPort>
        </interfaceReference>
      </interfacereferences>
    </implementation>
  </implements>
</serviceModel>