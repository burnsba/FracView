# FracViewCmd

Fractal origin is given by Origin.X and Origin.Y. The size of the fractal area to render is given by FractalWidth and FractalHeight. The area is divided into individual pixels, the width and height being given by StepWidth and StepHeight. For each pixel, the escape algorithm is evaluated at that (fractal) location, up to the max number of iteration steps.

Note: The json settings file is the only way to specify color information for the command line application.

If an option is specified in both the json file and from the command line, the command line value will be used.

The following options are required to be specified, either from the command line or from a json file:

- OriginX
- OriginY
- FractalWidth
- FractalHeight
- StepWidth
- StepHeight
- MaxIterations

Help:

```

  -x, --originx          World origin x location.
  -y, --originy          World origin y location.
  --fractalwidth         Fractal width.
  --fractalheight        Fractal height.
  --stepwidth            Set output pixel resolution width.
  --stepheight           Set output pixel resolution height.
  -i, --maxiterations    Max number of iterations.
  -h, --usehistogram     (Default: true) Whether or not to calculate histogram data.
  -f, --format           (Default: png) Output file format. Supported formats: bmp,gif,jpg,png.
  --report               (Default: 5) Progress report interval in seconds.
  -q, --quiet            (Default: false) Disable output progress.
  -o, --output           Output filename. Defaults to run timestamp. Provided extension will be
                         ignored; resolved based on format.
  -m, --meta             (Default: false) Write metadata file in addition to image.
  -a, --algorithm        (Default: FracView.Algorithms.MandelbrotDouble) Name of class to
                         instantiate. FracView.Algorithms namespace is assumed.
  -j, --json             Path to json settings file. Compatible with wpf app saved session. Command
                         line options will override json.
  --help                 Display this help screen.
  --version              Display version information.

Example:
  FracViewCmd --fractalheight 0.250 --fractalwidth 0.250 --usehistogram --maxiterations 1000 --stepheight 512 --stepwidth 512 --originx 0.29999999799999 --originy 0.4491000000000016
```

Example settings json file

```
{
  "RunSettings": {
    "OriginX": 0.3400911291793750947561553029,
    "OriginY": 0.5743346282087257566051136364,
    "FractalWidth": 0.0013774104683195592286501377,
    "FractalHeight": 0.0013774104683195592286501377,
    "StepWidth": 2048,
    "StepHeight": 2048,
    "MaxIterations": 4000,
    "UseHistogram": true
  },
  "StableColor": "#ffffff00",
  "ColorRampKeyframes": [
    {
      "IntervalStart": 0.0,
      "IntervalEnd": 0.88,
      "ValueStart": "#ffff0000",
      "ValueEnd": "#ff14fafa"
    },
    {
      "IntervalStart": 0.88,
      "IntervalEnd": 0.97,
      "ValueStart": "#ff14fafa",
      "ValueEnd": "#ffffff28"
    },
    {
      "IntervalStart": 0.97,
      "IntervalEnd": 0.99,
      "ValueStart": "#ffffff28",
      "ValueEnd": "#fffa8000"
    },
    {
      "IntervalStart": 0.99,
      "IntervalEnd": 1.0,
      "ValueStart": "#fffa8000",
      "ValueEnd": "#ff783c00"
    }
  ]
}
```