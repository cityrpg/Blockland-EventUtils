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

// SimObject::onInputEventProcess
// Called immediately before the input event takes effect.
// These parameters will often match, i.e. in client events, the target object is always the same as the triggering client.
// this: The object that the event is targeting.
// obj: The object the event has been called from.
// client: The client object that fired the event, if any.
// %outputEvent: The function being called.
function SimObject::onInputEventProcess(%this, %obj, %client, %outputEvent)
{
  if(isObject(%client))
  {
    %client.lastEventObject = %obj;
  }
}

// Source: https://github.com/Electrk/bl-decompiled/blob/master/server/scripts/allGameScripts.cs#L128

// SimObject::processInputEvent
// Oh boy! I can forsee absolutely nothing at all that could possibly go wrong here.
// We're entirely overwriting the function as defined in the base game.
// This will break anything and everything that packages this function *before* EventUtils executes, hence why preloading is required.
// Modified lines are tagged a comment starting with "eventUtils"
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
    if(%obj.eventInput[%i] $= %EventName && %obj.eventEnabled[%i])
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
    if (%obj.eventTarget[%i] == -1 && %obj.eventDelay[%i] <= 0 && %obj.eventOutput[%i] $= "CancelEvents" && %obj.eventInput[%i] $= %EventName && %obj.eventEnabled[%i])
    {
      %name = %obj.eventNT[%i];
      %group = %obj.getGroup ();
      %j = 0;
      while (%j < %group.NTObjectCount[%name])
      {
        %target = %group.NTObject[%name, %j];
        if(isObject (%target)) 
        {
          %target.cancelEvents ();
        }
        %j += 1;
      }
    }
    else 
    {
      %target = $InputTarget_[%obj.eventTarget[%i]];
      if(isObject (%target))
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
    if (%obj.eventTarget[%i] == -1 && %obj.eventOutput[%i] !$= "CancelEvents" && %obj.eventDelay[%i] != 0 && %obj.eventEnabled[%i] && %obj.eventInput[%i] $= %EventName)
    {
      %name = %obj.eventNT[%i];
      %group = %obj.getGroup ();
      %j = 0;
      while (%j < %group.NTObjectCount[%name])
      {
        %target = %group.NTObject[%name, %j];
        if(isObject (%target)) 
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
    if(%obj.eventOutput[%i] !$= "CancelEvents" && %obj.eventDelay[%i] != 0 && %obj.eventEnabled[%i] &&%obj.eventInput[%i] !$= %EventName) 
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
          if (isObject (%target))
          {
            %targetClass = "fxDTSBrick";
            %numParameters = outputEvent_GetNumParametersFromIdx (%targetClass, %outputEventIdx);

            // eventUtils: Schedule the onInputEventProcess call
            %target.schedule(%delay, onInputEventProcess, %obj, %client, %outputEvent);

            if (%obj.eventOutputAppendClient[%i])
            {
              if (%numParameters == 0)
              {
                %scheduleID = %target.schedule (%delay, %outputEvent, %client);
              }
              else if (%numParameters == 1)
              {
                %scheduleID = %target.schedule (%delay, %outputEvent, %par1, %client);
              }
              else if (%numParameters == 2)
              {
                %scheduleID = %target.schedule (%delay, %outputEvent, %par1, %par2, %client);
              }
              else if (%numParameters == 3)
              {
                %scheduleID = %target.schedule (%delay, %outputEvent, %par1, %par2, %par3, %client);
              }
              else if (%numParameters == 4)
              {
                %scheduleID = %target.schedule (%delay, %outputEvent, %par1, %par2, %par3, %par4, %client);
              }
              else 
              {
                error ("ERROR: SimObject::ProcessInputEvent() - bad number of parameters on event \'" @ %outputEvent @ "\' (" @ numParameters @ ")");
              }
            }
            else if (%numParameters == 0)
            {
              %scheduleID = %target.schedule (%delay, %outputEvent);
            }
            else if (%numParameters == 1)
            {
              %scheduleID = %target.schedule (%delay, %outputEvent, %par1);
            }
            else if (%numParameters == 2)
            {
              %scheduleID = %target.schedule (%delay, %outputEvent, %par1, %par2);
            }
            else if (%numParameters == 3)
            {
              %scheduleID = %target.schedule (%delay, %outputEvent, %par1, %par2, %par3);
            }
            else if (%numParameters == 4)
            {
              %scheduleID = %target.schedule (%delay, %outputEvent, %par1, %par2, %par3, %par4);
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
        if(isObject (%target)) 
        {
          %targetClass = inputEvent_GetTargetClass ("fxDTSBrick", %obj.eventInputIdx[%i], %obj.eventTargetIdx[%i]);
          %numParameters = outputEvent_GetNumParametersFromIdx (%targetClass, %outputEventIdx);

          // eventUtils: Schedule the onInputEventProcess call
          %target.schedule(%delay, onInputEventProcess, %obj, %client, %outputEvent);

          if (%obj.eventOutputAppendClient[%i])
          {
            if (%numParameters == 0)
            {
              %scheduleID = %target.schedule (%delay, %outputEvent, %client);
            }
            else if (%numParameters == 1)
            {
              %scheduleID = %target.schedule (%delay, %outputEvent, %par1, %client);
            }
            else if (%numParameters == 2)
            {
              %scheduleID = %target.schedule (%delay, %outputEvent, %par1, %par2, %client);
            }
            else if (%numParameters == 3)
            {
              %scheduleID = %target.schedule (%delay, %outputEvent, %par1, %par2, %par3, %client);
            }
            else if (%numParameters == 4)
            {
              %scheduleID = %target.schedule (%delay, %outputEvent, %par1, %par2, %par3, %par4, %client);
            }
            else 
            {
              error ("ERROR: SimObject::ProcessInputEvent() - bad number of parameters on event \'" @ %outputEvent @ "\' (" @ numParameters @ ")");
            }
          }
          else if (%numParameters == 0)
          {
            %scheduleID = %target.schedule (%delay, %outputEvent);
          }
          else if (%numParameters == 1)
          {
            %scheduleID = %target.schedule (%delay, %outputEvent, %par1);
          }
          else if (%numParameters == 2)
          {
            %scheduleID = %target.schedule (%delay, %outputEvent, %par1, %par2);
          }
          else if (%numParameters == 3)
          {
            %scheduleID = %target.schedule (%delay, %outputEvent, %par1, %par2, %par3);
          }
          else if (%numParameters == 4)
          {
            %scheduleID = %target.schedule (%delay, %outputEvent, %par1, %par2, %par3, %par4);
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