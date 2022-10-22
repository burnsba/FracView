## FracViewWpf technical documentation

### Code layout

**Constants**

Contains runtime constants used by the application.

**Controls**

WPF controls.

**Converters**

Helper classes to transform objects into other objects. This can be `IValueConverter` used by WPF, or transforming exception into a more text-friendly format.

**Dto**

Simple data transfer objects.

**Mvvm**

Generic MVVM classes, not particular to any project.

**ViewModels**

View models used by the applications, contains most of the application logic.

**Windows**

Windows (WPF).

**Workspace.cs**

Singleton instance used to grab things such as the dependency injection service. Contains helper methods to instantiate windows.

### Implementation

Fractal origin is given by Origin.X and Origin.Y. The size of the fractal area to render is given by FractalWidth and FractalHeight. The area is divided into individual pixels, the width and height being given by StepWidth and StepHeight. For each pixel, the escape algorithm is evaluated at that (fractal) location, up to the max number of iteration steps.

The user interface textboxes are mapped to string values in the viewmodel. On change, these values are attempted to be parsed as decimal. On success, the underlying parameters will be updated, otherwise a validation error will be indicated on the user interface. When running the algorithm, the last valid input parameters will be used.

Panning and zooming the image are managed in the MainWindow code behind. These events are forwarded to the viewmodel (with UI state) to update the position stats on the left side of the screen. (There is a minor zoom discrepency I cannot resolve that moves the center pixel on zoom levels close to 1.0). Zooming does not recalculate point data, it only affects UI scale. Zooming in will result in an increasingly blurry image until point data is recalculated.

The algorithm is computed on a background thread. The application manages a simple state machine of algorithm computation. This is used to resolve whether to send the cancellation token or to start a new processing task.

Currently the WPF does not have a method of selecting the algorithm to run.