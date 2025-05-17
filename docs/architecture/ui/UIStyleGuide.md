# UI Style Guide

This document outlines the UI style guidelines for the Wendover HOA application, ensuring a consistent, accessible, and professional visual design across all components of the application.

## Brand Identity

### Logo

![Wendover HOA Logo](../../assets/images/logo.png)

- **Primary Logo**: Full color version for headers and primary branding
- **Secondary Logo**: Monochrome version for footers and secondary uses
- **Favicon**: Simplified version for browser tabs and bookmarks
- **Minimum Size**: Logo should never appear smaller than 32px in height
- **Clear Space**: Maintain padding around the logo equal to 25% of its height

### Color Palette

#### Primary Colors

| Color Name | Hex Code | RGB | Usage |
|------------|----------|-----|-------|
| Primary Blue | #0056b3 | rgb(0, 86, 179) | Primary buttons, links, header |
| Secondary Green | #28a745 | rgb(40, 167, 69) | Success states, confirmation buttons |
| Accent Orange | #fd7e14 | rgb(253, 126, 20) | Highlights, calls to action |
| Neutral Gray | #6c757d | rgb(108, 117, 125) | Secondary text, borders |

#### Secondary Colors

| Color Name | Hex Code | RGB | Usage |
|------------|----------|-----|-------|
| Light Blue | #e9f2ff | rgb(233, 242, 255) | Backgrounds, hover states |
| Light Green | #e6f7e9 | rgb(230, 247, 233) | Success backgrounds |
| Light Orange | #fff3e6 | rgb(255, 243, 230) | Warning backgrounds |
| Light Gray | #f8f9fa | rgb(248, 249, 250) | Page backgrounds, cards |

#### Functional Colors

| Color Name | Hex Code | RGB | Usage |
|------------|----------|-----|-------|
| Success | #28a745 | rgb(40, 167, 69) | Success messages, completed states |
| Warning | #ffc107 | rgb(255, 193, 7) | Warning messages, alerts |
| Danger | #dc3545 | rgb(220, 53, 69) | Error messages, destructive actions |
| Info | #17a2b8 | rgb(23, 162, 184) | Information messages, help text |

### Typography

#### Font Families

- **Primary Font**: Roboto
- **Secondary Font**: Open Sans
- **Fallback Fonts**: -apple-system, BlinkMacSystemFont, "Segoe UI", "Helvetica Neue", Arial, sans-serif

```css
body {
  font-family: 'Roboto', -apple-system, BlinkMacSystemFont, "Segoe UI", "Helvetica Neue", Arial, sans-serif;
}

h1, h2, h3, h4, h5, h6 {
  font-family: 'Open Sans', -apple-system, BlinkMacSystemFont, "Segoe UI", "Helvetica Neue", Arial, sans-serif;
}
```

#### Font Sizes

| Element | Size | Weight | Line Height | Usage |
|---------|------|--------|-------------|-------|
| h1 | 2.5rem (40px) | 700 | 1.2 | Page titles |
| h2 | 2rem (32px) | 700 | 1.2 | Section headers |
| h3 | 1.75rem (28px) | 600 | 1.3 | Subsection headers |
| h4 | 1.5rem (24px) | 600 | 1.4 | Card headers |
| h5 | 1.25rem (20px) | 600 | 1.4 | Widget titles |
| h6 | 1rem (16px) | 600 | 1.5 | Small section headers |
| Body | 1rem (16px) | 400 | 1.5 | Main content text |
| Small | 0.875rem (14px) | 400 | 1.5 | Secondary text, captions |
| XSmall | 0.75rem (12px) | 400 | 1.5 | Legal text, footnotes |

```css
h1 {
  font-size: 2.5rem;
  font-weight: 700;
  line-height: 1.2;
}

body {
  font-size: 1rem;
  font-weight: 400;
  line-height: 1.5;
}
```

## Layout

### Grid System

The Wendover HOA application uses Bootstrap's 12-column grid system for responsive layouts.

- **Container**: `.container` for fixed-width or `.container-fluid` for full-width
- **Row**: `.row` to create horizontal groups of columns
- **Columns**: `.col-*` classes for responsive column widths

```html
<div class="container">
  <div class="row">
    <div class="col-md-8">Main content</div>
    <div class="col-md-4">Sidebar</div>
  </div>
</div>
```

### Breakpoints

| Breakpoint | Class Prefix | Device Type | Screen Width |
|------------|-------------|-------------|--------------|
| Extra Small | .col- | Mobile phones | < 576px |
| Small | .col-sm- | Large phones, small tablets | ≥ 576px |
| Medium | .col-md- | Tablets | ≥ 768px |
| Large | .col-lg- | Desktops | ≥ 992px |
| Extra Large | .col-xl- | Large desktops | ≥ 1200px |
| Extra Extra Large | .col-xxl- | Very large desktops | ≥ 1400px |

### Spacing

The application uses a consistent spacing system based on Bootstrap's spacing utilities:

| Size | Rem | Pixels | Class Examples |
|------|-----|--------|---------------|
| 0 | 0 | 0px | .m-0, .p-0 |
| 1 | 0.25rem | 4px | .mt-1, .pb-1 |
| 2 | 0.5rem | 8px | .my-2, .px-2 |
| 3 | 1rem | 16px | .ms-3, .pe-3 |
| 4 | 1.5rem | 24px | .mb-4, .pt-4 |
| 5 | 3rem | 48px | .me-5, .ps-5 |

```html
<div class="mt-3 mb-5">
  <h2 class="mb-4">Section Title</h2>
  <p class="mb-3">Content with consistent spacing</p>
</div>
```

## Components

### Buttons

#### Button Styles

| Style | Class | Usage |
|-------|-------|-------|
| Primary | .btn-primary | Main actions, form submissions |
| Secondary | .btn-secondary | Alternative actions, cancellations |
| Success | .btn-success | Positive actions, confirmations |
| Danger | .btn-danger | Destructive actions, deletions |
| Warning | .btn-warning | Cautionary actions |
| Info | .btn-info | Informational actions |
| Light | .btn-light | Subtle actions on dark backgrounds |
| Dark | .btn-dark | Subtle actions on light backgrounds |
| Link | .btn-link | Text-like buttons |

#### Button Sizes

| Size | Class | Usage |
|------|-------|-------|
| Large | .btn-lg | Primary page actions |
| Default | (none) | Standard actions |
| Small | .btn-sm | Secondary or compact actions |

```html
<button type="button" class="btn btn-primary">Primary Action</button>
<button type="button" class="btn btn-secondary">Secondary Action</button>
<button type="button" class="btn btn-danger btn-sm">Delete</button>
```

### Forms

#### Form Controls

- **Text Inputs**: Use `.form-control` for text inputs, textareas, and selects
- **Labels**: Use `.form-label` for form labels
- **Help Text**: Use `.form-text` for additional guidance
- **Validation**: Use `.is-valid` and `.is-invalid` with `.valid-feedback` and `.invalid-feedback`

```html
<div class="mb-3">
  <label for="email" class="form-label">Email address</label>
  <input type="email" class="form-control" id="email" aria-describedby="emailHelp">
  <div id="emailHelp" class="form-text">We'll never share your email with anyone else.</div>
</div>
```

#### Form Layout

- **Default**: Vertical form layout (default)
- **Horizontal**: Use `.row` and column classes for horizontal forms
- **Inline**: Use `.row-cols-*` and `.g-*` for inline forms

```html
<!-- Horizontal form -->
<div class="row mb-3">
  <label for="email" class="col-sm-2 col-form-label">Email</label>
  <div class="col-sm-10">
    <input type="email" class="form-control" id="email">
  </div>
</div>
```

### Cards

Use Bootstrap cards for content containers with consistent styling.

```html
<div class="card">
  <div class="card-header">
    Featured
  </div>
  <div class="card-body">
    <h5 class="card-title">Special title treatment</h5>
    <p class="card-text">With supporting text below as a natural lead-in to additional content.</p>
    <a href="#" class="btn btn-primary">Go somewhere</a>
  </div>
  <div class="card-footer text-muted">
    2 days ago
  </div>
</div>
```

### Tables

Use Bootstrap tables for tabular data with consistent styling.

```html
<table class="table table-striped">
  <thead>
    <tr>
      <th scope="col">#</th>
      <th scope="col">First</th>
      <th scope="col">Last</th>
      <th scope="col">Handle</th>
    </tr>
  </thead>
  <tbody>
    <tr>
      <th scope="row">1</th>
      <td>Mark</td>
      <td>Otto</td>
      <td>@mdo</td>
    </tr>
    <!-- Additional rows -->
  </tbody>
</table>
```

### Alerts

Use Bootstrap alerts for feedback messages.

```html
<div class="alert alert-success" role="alert">
  <h4 class="alert-heading">Well done!</h4>
  <p>Your changes have been saved successfully.</p>
</div>
```

### Navigation

#### Navbar

Use Bootstrap navbar for consistent site navigation.

```html
<nav class="navbar navbar-expand-lg navbar-dark bg-primary">
  <div class="container-fluid">
    <a class="navbar-brand" href="#">Wendover HOA</a>
    <button class="navbar-toggler" type="button" data-bs-toggle="collapse" data-bs-target="#navbarNav">
      <span class="navbar-toggler-icon"></span>
    </button>
    <div class="collapse navbar-collapse" id="navbarNav">
      <ul class="navbar-nav">
        <li class="nav-item">
          <a class="nav-link active" aria-current="page" href="#">Home</a>
        </li>
        <!-- Additional nav items -->
      </ul>
    </div>
  </div>
</nav>
```

#### Breadcrumbs

Use breadcrumbs for page hierarchy navigation.

```html
<nav aria-label="breadcrumb">
  <ol class="breadcrumb">
    <li class="breadcrumb-item"><a href="#">Home</a></li>
    <li class="breadcrumb-item"><a href="#">Documents</a></li>
    <li class="breadcrumb-item active" aria-current="page">Bylaws</li>
  </ol>
</nav>
```

## Icons

The Wendover HOA application uses Bootstrap Icons for consistent iconography.

```html
<i class="bi bi-house-fill"></i> Home
<i class="bi bi-person-circle"></i> Profile
<i class="bi bi-calendar-event"></i> Events
```

### Common Icons

| Purpose | Icon | Class |
|---------|------|-------|
| Home | <i class="bi bi-house-fill"></i> | bi-house-fill |
| User/Profile | <i class="bi bi-person-circle"></i> | bi-person-circle |
| Calendar/Events | <i class="bi bi-calendar-event"></i> | bi-calendar-event |
| Documents | <i class="bi bi-file-text"></i> | bi-file-text |
| Settings | <i class="bi bi-gear"></i> | bi-gear |
| Notifications | <i class="bi bi-bell"></i> | bi-bell |
| Search | <i class="bi bi-search"></i> | bi-search |
| Edit | <i class="bi bi-pencil"></i> | bi-pencil |
| Delete | <i class="bi bi-trash"></i> | bi-trash |
| Add | <i class="bi bi-plus-circle"></i> | bi-plus-circle |

## Accessibility

### Color Contrast

- All text must maintain a minimum contrast ratio of 4.5:1 against its background
- Large text (18pt or 14pt bold) must maintain a minimum contrast ratio of 3:1
- UI components and graphical objects must maintain a minimum contrast ratio of 3:1

### Focus States

- All interactive elements must have a visible focus state
- Do not remove the default focus outline unless replacing it with an equally visible alternative
- Focus order should follow a logical sequence

```css
:focus {
  outline: 2px solid #0056b3;
  outline-offset: 2px;
}
```

### ARIA Attributes

- Use appropriate ARIA roles, states, and properties when native HTML semantics are insufficient
- Include `aria-label` or `aria-labelledby` for elements without visible text labels
- Use `aria-expanded`, `aria-hidden`, and `aria-controls` for interactive components

```html
<button aria-expanded="false" aria-controls="collapseExample">
  Toggle Content
</button>
<div id="collapseExample" class="collapse">
  <div class="card card-body">
    Collapsible content
  </div>
</div>
```

## Responsive Design

### Mobile-First Approach

- Design for mobile devices first, then enhance for larger screens
- Use Bootstrap's responsive grid system and utility classes
- Test all features on multiple device sizes

### Touch Targets

- Minimum touch target size: 44x44 pixels
- Minimum spacing between touch targets: 8 pixels
- Ensure all interactive elements are easily tappable on mobile devices

### Responsive Images

- Use the `img-fluid` class for responsive images
- Consider using `picture` element or srcset for art direction and resolution switching

```html
<img src="image.jpg" class="img-fluid" alt="Responsive image">
```

## Implementation Guidelines

### CSS Organization

- Use Bootstrap's built-in classes whenever possible
- Organize custom CSS using the BEM (Block, Element, Modifier) methodology
- Maintain a separate custom.css file for overrides and extensions

```css
/* Block component */
.card {
}

/* Element that depends upon the block */
.card__title {
}

/* Modifier that changes the style of the block */
.card--featured {
}
```

### Component Implementation

- Create reusable Razor components for common UI elements
- Document component usage with examples and parameters
- Maintain consistency with Bootstrap's component structure

```csharp
@* ExampleComponent.razor *@
<div class="card @(Featured ? "card--featured" : "")">
    <div class="card-header">@Title</div>
    <div class="card-body">
        @ChildContent
    </div>
</div>

@code {
    [Parameter]
    public string Title { get; set; }
    
    [Parameter]
    public bool Featured { get; set; }
    
    [Parameter]
    public RenderFragment ChildContent { get; set; }
}
```

## Theming

### Customizing Bootstrap

- Use Bootstrap's built-in Sass variables for theming
- Maintain a separate _variables.scss file for custom variable overrides
- Compile custom Bootstrap build to reduce file size

```scss
// _variables.scss
$primary: #0056b3;
$secondary: #6c757d;
$success: #28a745;
$info: #17a2b8;
$warning: #ffc107;
$danger: #dc3545;
$light: #f8f9fa;
$dark: #343a40;

// Import Bootstrap
@import "bootstrap/scss/bootstrap";
```

### Dark Mode

- Implement dark mode using CSS variables
- Provide a toggle for users to switch between light and dark modes
- Test all components in both modes

```css
:root {
  --bg-color: #ffffff;
  --text-color: #212529;
  --card-bg: #f8f9fa;
}

[data-theme="dark"] {
  --bg-color: #121212;
  --text-color: #e0e0e0;
  --card-bg: #1e1e1e;
}

body {
  background-color: var(--bg-color);
  color: var(--text-color);
}

.card {
  background-color: var(--card-bg);
}
```

## Conclusion

This UI Style Guide provides a comprehensive reference for maintaining a consistent visual design across the Wendover HOA application. By following these guidelines, developers can ensure that all components adhere to the established design system, resulting in a cohesive and professional user interface.

For implementation details and component examples, refer to the [Component Library](./ComponentLibrary.md) document.
