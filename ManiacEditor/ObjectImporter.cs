﻿using ManiacEditor.Properties;
using RSDKv5;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ManiacEditor
{
    public partial class ObjectImporter : Form
    {
        private IList<SceneObject> _sourceSceneObjects;
        private IList<SceneObject> _targetSceneObjects;
        private StageConfig _stageConfig;

        public ObjectImporter(IList<SceneObject> sourceSceneObjects, IList<SceneObject> targetSceneObjects, StageConfig stageConfig)
        {
            InitializeComponent();
            rtbWarning.Rtf = Resources.ObjectWarning;
            _sourceSceneObjects = sourceSceneObjects;
            _targetSceneObjects = targetSceneObjects;
            _stageConfig = stageConfig;

            var targetNames = targetSceneObjects.Select(tso => tso.Name.ToString());
            var importableObjects = sourceSceneObjects.Where(sso => !targetNames.Contains(sso.Name.ToString()))
                                                        .OrderBy(sso => sso.Name.ToString());
            if (Properties.Settings.Default.RemoveObjectImportLock == true) {
                importableObjects = sourceSceneObjects.Where(sso => targetNames.Contains(sso.Name.ToString()))
                                                        .OrderBy(sso => sso.Name.ToString());
            }
            else 
            {
                importableObjects = sourceSceneObjects.Where(sso => !targetNames.Contains(sso.Name.ToString()))
                                        .OrderBy(sso => sso.Name.ToString());
            }

            foreach (var io in importableObjects)
            {
                var lvi = new ListViewItem(io.Name.ToString())
                {
                    Checked = false
                };
                
                lvObjects.Items.Add(lvi);
            }
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            Close();
        }

        private void btnImport_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.OK;

            foreach (var lvci in lvObjects.CheckedItems)
            {
                var item = lvci as ListViewItem;
                SceneObject objectToImport = _sourceSceneObjects.Single(sso => sso.Name.ToString().Equals(item.Text));
                objectToImport.Entities.Clear(); // ditch instances of the object from the imported level
                _targetSceneObjects.Add(_sourceSceneObjects.Single(sso => sso.Name.ToString().Equals(item.Text)));

                if (   _stageConfig != null 
                    && !_stageConfig.ObjectsNames.Contains(item.Text))
                {
                    _stageConfig.ObjectsNames.Add(item.Text);
                }
            }

            Close();
        }
    }
}
