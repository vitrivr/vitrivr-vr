using System;
using System.Collections.Generic;
using UnityEngine;
using Vitrivr.UnityInterface.CineastApi.Model.Config;

namespace VitrivrVR.Config
{
  [Serializable]
  public class BooleanCategory
  {
    public string name, selectionType, table, column;
    public string[] options;
  }

  [Serializable]
  public class VitrivrVrConfig
  {
    [Serializable]
    public class ConfigColor
    {
      public float r, g, b;

      public ConfigColor(float r, float g, float b)
      {
        this.r = r;
        this.g = g;
        this.b = b;
      }

      public Color ToColor() => new(r, g, b);
    }

    [Serializable]
    public class TextCategory
    {
      public string name, id;

      public TextCategory(string name, string id)
      {
        this.name = name;
        this.id = id;
      }
    }

    [Serializable]
    public enum SpeechToText
    {
      DeepSpeech,
      Whisper
    }

    /// <summary>
    /// List of paths relative to persistent path pointing to all enabled Cineast instances.
    /// </summary>
    public List<string> cineastConfigs;

    /// <summary>
    /// The maximum number of results to accept from a single query.
    /// </summary>
    public int maxResults;

    /// <summary>
    /// The maximum number of items to display at once.
    /// </summary>
    public int maxDisplay;

    /// <summary>
    /// The color to indicate the minimum score for returned results.
    /// </summary>
    public ConfigColor dissimilarityColor;

    /// <summary>
    /// The color to indicate the maximum score for returned results.
    /// </summary>
    public ConfigColor similarityColor;

    /// <summary>
    /// The default categories for image query-by-example.
    /// </summary>
    public List<string> defaultImageCategories;

    /// <summary>
    /// The text categories to provide for text query terms.
    /// Leave empty to disable.
    /// </summary>
    public List<TextCategory> textCategories;

    /// <summary>
    /// The categories for Boolean terms.
    /// Leave empty to disable.
    /// </summary>
    public List<BooleanCategory> booleanCategories;

    public bool tagTerm;

    public bool mapTerm;

    public bool poseTerm;

    /// <summary>
    /// The default volume [0, 1] to use for audio and video.
    /// </summary>
    public float defaultMediaVolume;

    /// <summary>
    /// The default length in seconds to skip using the skip controls for audio and video.
    /// </summary>
    public float skipLength;

    /// <summary>
    /// The default enabled speech-to-text method.
    /// </summary>
    public SpeechToText defaultSpeechToText;

    /// <summary>
    /// The default setting of whether to create a point cloud display for a query result.
    /// </summary>
    public bool createPointCloud;

    /// <summary>
    /// The maximum number of points to display in the point cloud.
    /// </summary>
    public int pointCloudPointLimit;

    /// <summary>
    /// The feature (more specifically feature table name) to use for dimensionality reduction.
    /// </summary>
    public string pointCloudFeature;

    // TODO: Include configuration of point cloud dimensionality reduction

    /// <summary>
    /// Enables or disables DRES submission system integration and all associated functionality.
    /// </summary>
    public bool dresEnabled;

    /// <summary>
    /// Enables or disables text submission window (only enables if DRES is enabled as well).
    /// </summary>
    public bool textSubmissionEnabled;

    /// <summary>
    /// Enables connecting to a DRES instance with an invalid SSL certificate.
    /// WARNING: This bypasses certificate validation, use at your own risk!
    /// </summary>
    public bool allowInvalidCertificate;

    /// <summary>
    /// A regex matching any part of the media object ID to be removed before submission.
    /// If e.g. internal IDs start with "v_" while competition IDs do not, this parameter may be set to "^v_" to remove
    /// this for submission to the competition system only.
    /// </summary>
    public string submissionIdReplacementRegex;

    /// <summary>
    /// The time interval between sending interaction logs to DRES.
    /// </summary>
    public float interactionLogSubmissionInterval;

    /// <summary>
    /// Enables writing results and interaction logs to file.
    /// </summary>
    public bool writeLogsToFile;

    /// <summary>
    /// Path to location where log files are to be written.
    /// </summary>
    public string logFileLocation;
    
    /// <summary>
    /// Enables a fix for the Vive streaming bug by periodically disabling and re-enabling UI interactions when no
    /// interactions are detected.
    /// </summary>
    public bool viveStreamingFixEnabled;

    private VitrivrVrConfig()
    {
      cineastConfigs = new List<string> {"cineastapi.json"};
      maxResults = 10000;
      maxDisplay = 100;
      dissimilarityColor = new ConfigColor(1, 0, 0);
      similarityColor = new ConfigColor(0, 1, 0);
      defaultImageCategories = new List<string>
      {
        CategoryMappings.GlobalColorCategory,
        CategoryMappings.EdgeCategory
      };
      textCategories = new List<TextCategory>
      {
        new("OCR", "ocr"),
        new("ASR", "asr"),
        new("Caption", "scenecaption"),
        new("Co-Embed", "visualtextcoembedding")
      };
      booleanCategories = new List<BooleanCategory>();
      tagTerm = false;
      mapTerm = false;
      poseTerm = false;
      defaultMediaVolume = .5f;
      skipLength = 2.5f;
      defaultSpeechToText = SpeechToText.Whisper;
      createPointCloud = true;
      pointCloudPointLimit = 2000;
      pointCloudFeature = "openclip";
      dresEnabled = false;
      textSubmissionEnabled = true;
      allowInvalidCertificate = false;
      submissionIdReplacementRegex = "";
      interactionLogSubmissionInterval = 10f;
      writeLogsToFile = false;
      logFileLocation = "session_logs/";
      viveStreamingFixEnabled = false;
    }

    public static VitrivrVrConfig GetDefault()
    {
      return new VitrivrVrConfig();
    }
  }
}