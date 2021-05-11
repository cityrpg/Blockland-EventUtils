// We have to tackle the rather dangerous task of outright overwiting a base game function.
// To do this as safely as possible, we're pre-loading EventUtils on server start, similar to Blockland Glass.

package EventUtilsPreloader
{
  function deactivateServerPackages()
  {
    parent::deactivateServerPackages();

    if($Library::LastEvent::Loaded)
    {
      // Server is shutting down
      $Library::LastEvent::Loaded = 0;
      return;
    }

    // TODO: Check if enabled?
    if(%enabled)
    {
      exec("./Support_EventUtils.cs");
    }
  }
};

activatePackage("EventUtilsPreloader");
 