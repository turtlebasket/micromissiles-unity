module.exports = {
  title: 'Micromissiles Unity Project',
  description: 'Documentation for the Micromissiles Unity project',
  themeConfig: {
    navbar: [
      { text: 'Home', link: '/' },
      { text: 'Keybinds and Controls', link: '/Keybinds_and_Controls.html' },
      { text: 'Simulation Configuration Guide', link: '/Simulation_Config_Guide.html' },
      { text: 'Simulation Logging', link: '/Simulation_Logging.html' },
    ],
    sidebar: 'auto',
  },
  extendsPageData: (pageData) => {
    if (pageData.relativePath === 'README.md') {
      pageData.frontmatter.home = true;
    }
  },
}