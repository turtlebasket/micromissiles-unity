module.exports = {
  title : 'Micromissiles Unity Project',
  description : 'Documentation for the Micromissiles Unity project',
  base : '/micromissiles-unity/',
  themeConfig : {
    navbar :
           [
             {text : 'Home', link : '/'}, {
               text : 'Keybinds and Controls',
               link : '/Keybinds_and_Controls.html'
             },
             {
               text : 'Simulation Configuration Guide',
               link : '/Simulation_Config_Guide.html'
             },
             {text : 'Simulation Logging', link : '/Simulation_Logging.html'}, {
               text : 'Coverage Reports',
               link : '/coverage/'
             },  // Link to coverage reports
           ],
    sidebar : 'auto',
  },
  // Add this to ensure INDEX.md is used as the home page
  patterns : ['INDEX.md', '**/*.md', '**/*.vue'],
}