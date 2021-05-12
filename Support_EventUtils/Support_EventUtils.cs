//------------------------------------------------------------------------------
// Title:   Event Utils
// Author:  Lake
// Version: 2
// Updated: May 7, 2021
//------------------------------------------------------------------------------
// This *must* be loaded before other packages to avoid breaking them.
//------------------------------------------------------------------------------

// Version check
if($Library::LastEvent::Ver > 1.1)
	return;

$Library::LastEvent::Ver = 1.1;
$Library::LastEvent::Loaded = true;

deactivatePackage("Support_LastEvent");
package Support_LastEvent
{
  function SimObject::EventUtilsCallEvent(%obj, %outputEvent, %numParameters, %eventOutputAppendClient, %sourceObj, %fullPar1, %fullPar2, %fullPar3, %fullPar4, %fullPar5)
  {
    // Client object is the last parameter passed.
    %client = %fullPar[%numParameters+1];
    if(%eventOutputAppendClient && isObject(%client))
    {
      %client.lastEventObject = %sourceObj;
    }

    %obj.call(%outputEvent, %fullPar1, %fullPar2, %fullPar3, %fullPar4, %fullPar5);
  }

  // Source: https://github.com/Electrk/bl-decompiled/blob/master/server/scripts/allGameScripts.cs#L128
  
  // SimObject::processInputEvent
  // Oh boy! I can forsee absolutely nothing at all that could possibly go wrong here.
  // We're entirely overwriting the function as defined in the base game.
  // This will break anything and everything that packages this function *before* EventUtils executes, hence why preloading is required.
  function SimObject::processInputEvent (%obj, %EventName, %client)
  {
    if (%obj.numEvents <= 0)
    {
      return;
    }
    %foundOne = 0;
    %i = 0;
    while (%i < %obj.numEvents)
    {
      if (%obj.eventInput[%i] !$= %EventName)
      {
        
      }
      else if (!%obj.eventEnabled[%i])
      {
        
      }
      else 
      {
        %foundOne = 1;
        break;
      }
      %i += 1;
    }
    if (!%foundOne)
    {
      return;
    }
    if (isObject (%client))
    {
      %quotaObject = getQuotaObjectFromClient (%client);
    }
    else if (%obj.getType () & $TypeMasks::FxBrickAlwaysObjectType)
    {
      %quotaObject = getQuotaObjectFromBrick (%obj);
    }
    else 
    {
      if (getBuildString () !$= "Ship")
      {
        error ("ERROR: SimObject::ProcessInputEvent() - could not get quota object for event \"" @ %EventName @ "\" on object " @ %obj);
      }
      return;
    }
    if (!isObject (%quotaObject))
    {
      error ("ERROR: SimObject::ProcessInputEvent() - new quota object creation failed!");
    }
    setCurrentQuotaObject (%quotaObject);
    if (%EventName $= "OnRelay")
    {
      if (%obj.implicitCancelEvents)
      {
        %obj.cancelEvents ();
      }
    }
    %i = 0;
    while (%i < %obj.numEvents)
    {
      if (!%obj.eventEnabled[%i])
      {
        
      }
      else if (%obj.eventInput[%i] !$= %EventName)
      {
        
      }
      else if (%obj.eventOutput[%i] !$= "CancelEvents")
      {
        
      }
      else if (%obj.eventDelay[%i] > 0)
      {
        
      }
      else if (%obj.eventTarget[%i] == -1)
      {
        %name = %obj.eventNT[%i];
        %group = %obj.getGroup ();
        %j = 0;
        while (%j < %group.NTObjectCount[%name])
        {
          %target = %group.NTObject[%name, %j];
          if (!isObject (%target))
          {
            
          }
          else 
          {
            %target.cancelEvents ();
          }
          %j += 1;
        }
      }
      else 
      {
        %target = $InputTarget_[%obj.eventTarget[%i]];
        if (!isObject (%target))
        {
          
        }
        else 
        {
          %target.cancelEvents ();
        }
      }
      %i += 1;
    }
    %eventCount = 0;
    %i = 0;
    while (%i < %obj.numEvents)
    {
      if (%obj.eventInput[%i] !$= %EventName)
      {
        
      }
      else if (!%obj.eventEnabled[%i])
      {
        
      }
      else if (%obj.eventOutput[%i] $= "CancelEvents" && %obj.eventDelay[%i] == 0)
      {
        
      }
      else if (%obj.eventTarget[%i] == -1)
      {
        %name = %obj.eventNT[%i];
        %group = %obj.getGroup ();
        %j = 0;
        while (%j < %group.NTObjectCount[%name])
        {
          %target = %group.NTObject[%name, %j];
          if (!isObject (%target))
          {
            
          }
          else 
          {
            %eventCount += 1;
          }
          %j += 1;
        }
      }
      else 
      {
        %eventCount += 1;
      }
      %i += 1;
    }
    if (%eventCount == 0)
    {
      return;
    }
    %currTime = getSimTime ();
    if (%eventCount > %quotaObject.getAllocs_Schedules ())
    {
      commandToClient (%client, 'CenterPrint', "<color:FFFFFF>Too many events at once!\n(" @ %EventName @ ")", 1);
      if (%client.SQH_StartTime <= 0)
      {
        %client.SQH_StartTime = %currTime;
      }
      else 
      {
        if (%currTime - %client.SQH_LastTime < 2000)
        {
          %client.SQH_HitCount += 1;
        }
        if (%client.SQH_HitCount > 5)
        {
          %client.ClearEventSchedules ();
          %client.resetVehicles ();
          %mask = $TypeMasks::PlayerObjectType | $TypeMasks::ProjectileObjectType | $TypeMasks::VehicleObjectType | $TypeMasks::CorpseObjectType;
          %client.ClearEventObjects (%mask);
        }
      }
      %client.SQH_LastTime = %currTime;
      return;
    }
    if (%currTime - %client.SQH_LastTime > 1000)
    {
      %client.SQH_StartTime = 0;
      %client.SQH_HitCount = 0;
    }
    %i = 0;
    while (%i < %obj.numEvents)
    {
      if (%obj.eventInput[%i] !$= %EventName)
      {
        
      }
      else if (!%obj.eventEnabled[%i])
      {
        
      }
      else if (%obj.eventOutput[%i] $= "CancelEvents" && %obj.eventDelay[%i] == 0)
      {
        
      }
      else 
      {
        %delay = %obj.eventDelay[%i];
        %outputEvent = %obj.eventOutput[%i];
        %par1 = %obj.eventOutputParameter[%i, 1];
        %par2 = %obj.eventOutputParameter[%i, 2];
        %par3 = %obj.eventOutputParameter[%i, 3];
        %par4 = %obj.eventOutputParameter[%i, 4];
        %outputEventIdx = %obj.eventOutputIdx[%i];
        if (%obj.eventTarget[%i] == -1)
        {
          %name = %obj.eventNT[%i];
          %group = %obj.getGroup ();
          %j = 0;
          while (%j < %group.NTObjectCount[%name])
          {
            %target = %group.NTObject[%name, %j];
            if (!isObject (%target))
            {
              
            }
            else 
            {
              %targetClass = "fxDTSBrick";
              %numParameters = outputEvent_GetNumParametersFromIdx (%targetClass, %outputEventIdx);

              if (%obj.eventOutputAppendClient[%i])
              {
                if (%numParameters == 0)
                {
                  %scheduleID = %target.schedule (%delay, EventUtilsCallEvent, %outputEvent, %numParameters, true, %obj, %client);
                }
                else if (%numParameters == 1)
                {
                  %scheduleID = %target.schedule (%delay, EventUtilsCallEvent, %outputEvent, %numParameters, true, %obj, %par1, %client);
                }
                else if (%numParameters == 2)
                {
                  %scheduleID = %target.schedule (%delay, EventUtilsCallEvent, %outputEvent, %numParameters, true, %obj, %par1, %par2, %client);
                }
                else if (%numParameters == 3)
                {
                  %scheduleID = %target.schedule (%delay, EventUtilsCallEvent, %outputEvent, %numParameters, true, %obj, %par1, %par2, %par3, %client);
                }
                else if (%numParameters == 4)
                {
                  %scheduleID = %target.schedule (%delay, EventUtilsCallEvent, %outputEvent, %numParameters, true, %obj, %par1, %par2, %par3, %par4, %client);
                }
                else 
                {
                  error ("ERROR: SimObject::ProcessInputEvent() - bad number of parameters on event \'" @ %outputEvent @ "\' (" @ numParameters @ ")");
                }
              }
              else if (%numParameters == 0)
              {
                %scheduleID = %target.schedule (%delay, EventUtilsCallEvent, %outputEvent, %numParameters, false, %obj);
              }
              else if (%numParameters == 1)
              {
                %scheduleID = %target.schedule (%delay, EventUtilsCallEvent, %outputEvent, %numParameters, false, %obj, %par1);
              }
              else if (%numParameters == 2)
              {
                %scheduleID = %target.schedule (%delay, EventUtilsCallEvent, %outputEvent, %numParameters, false, %obj, %par1, %par2);
              }
              else if (%numParameters == 3)
              {
                %scheduleID = %target.schedule (%delay, EventUtilsCallEvent, %outputEvent, %numParameters, false, %obj, %par1, %par2, %par3);
              }
              else if (%numParameters == 4)
              {
                %scheduleID = %target.schedule (%delay, EventUtilsCallEvent, %outputEvent, %numParameters, false, %obj, %par1, %par2, %par3, %par4);
              }
              else 
              {
                error ("ERROR: SimObject::ProcessInputEvent() - bad number of parameters on event \'" @ %outputEvent @ "\' (" @ numParameters @ ")");
              }
              if (%delay > 0)
              {
                %obj.addScheduledEvent (%scheduleID);
              }
            }
            %j += 1;
          }
        }
        else 
        {
          %target = $InputTarget_[%obj.eventTarget[%i]];
          if (!isObject (%target))
          {
            
          }
          else 
          {
            %targetClass = inputEvent_GetTargetClass ("fxDTSBrick", %obj.eventInputIdx[%i], %obj.eventTargetIdx[%i]);
            %numParameters = outputEvent_GetNumParametersFromIdx (%targetClass, %outputEventIdx);

            if (%obj.eventOutputAppendClient[%i])
            {
              if (%numParameters == 0)
              {
                %scheduleID = %target.schedule (%delay, EventUtilsCallEvent, %outputEvent, %numParameters, true, %obj, %client);
              }
              else if (%numParameters == 1)
              {
                %scheduleID = %target.schedule (%delay, EventUtilsCallEvent, %outputEvent, %numParameters, true, %obj, %par1, %client);
              }
              else if (%numParameters == 2)
              {
                %scheduleID = %target.schedule (%delay, EventUtilsCallEvent, %outputEvent, %numParameters, true, %obj, %par1, %par2, %client);
              }
              else if (%numParameters == 3)
              {
                %scheduleID = %target.schedule (%delay, EventUtilsCallEvent, %outputEvent, %numParameters, true, %obj, %par1, %par2, %par3, %client);
              }
              else if (%numParameters == 4)
              {
                %scheduleID = %target.schedule (%delay, EventUtilsCallEvent, %outputEvent, %numParameters, true, %obj, %par1, %par2, %par3, %par4, %client);
              }
              else 
              {
                error ("ERROR: SimObject::ProcessInputEvent() - bad number of parameters on event \'" @ %outputEvent @ "\' (" @ numParameters @ ")");
              }
            }
            else if (%numParameters == 0)
            {
              %scheduleID = %target.schedule (%delay, EventUtilsCallEvent, %outputEvent, %numParameters, false, %obj);
            }
            else if (%numParameters == 1)
            {
              %scheduleID = %target.schedule (%delay, EventUtilsCallEvent, %outputEvent, %numParameters, false, %obj, %par1);
            }
            else if (%numParameters == 2)
            {
              %scheduleID = %target.schedule (%delay, EventUtilsCallEvent, %outputEvent, %numParameters, false, %obj, %par1, %par2);
            }
            else if (%numParameters == 3)
            {
              %scheduleID = %target.schedule (%delay, EventUtilsCallEvent, %outputEvent, %numParameters, false, %obj, %par1, %par2, %par3);
            }
            else if (%numParameters == 4)
            {
              %scheduleID = %target.schedule (%delay, EventUtilsCallEvent, %outputEvent, %numParameters, false, %obj, %par1, %par2, %par3, %par4);
            }
            else 
            {
              error ("ERROR: SimObject::ProcessInputEvent() - bad number of parameters on event \'" @ %outputEvent @ "\' (" @ numParameters @ ")");
            }
            if (%delay > 0 && %EventName !$= "onToolBreak")
            {
              %obj.addScheduledEvent (%scheduleID);
            }
          }
        }
      }
      %i += 1;
    }
  }
};
activatePackage("Support_LastEvent");
