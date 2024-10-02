module.exports = async () => {
  const {viteBundler} = await import('@vuepress/bundler-vite')

  return {
    title: 'Micromissiles Unity Project',
        description: 'Documentation for the Micromissiles Unity project',
        base: '/micromissiles-unity/', themeConfig: {
          navbar:
              [
                {text: 'Home', link: '/'}, {
                  text: 'Keybinds and Controls',
                  link: '/Keybinds_and_Controls/'
                },
                {
                  text: 'Simulation Configuration Guide',
                  link: '/Simulation_Config_Guide/'
                },
                {text: 'Simulation Logging', link: '/Simulation_Logging/'},
                {
                  text: 'Coverage Reports',
                  link: '/coverage/editmode/Report/'
                },  // Link to coverage reports
              ],
          sidebar: 'auto',
        },
        bundler: viteBundler(), 
        patterns: ['INDEX.md', '**/*.md', '**/*.vue'],
  }
}
