import { defineConfig } from 'vitepress'

// https://vitepress.dev/reference/site-config
export default defineConfig({
  title: "micromissiles-unity",
  description: "Swarm-on-swarm simulator using micromissiles for point defense",
  base: '/micromissiles-unity/',
  themeConfig: {
    // https://vitepress.dev/reference/default-theme-config
    nav: [
      { text: 'Home', link: '/' },
      { text: 'Documentation', link: '/Keybinds_and_Controls' },
      { text: 'Development Guide', link: '/Development_Guide' }
    ],

    sidebar: [
      {
        text: 'Documentation',
        items: [
          { text: 'Keybinds and Controls', link: '/Keybinds_and_Controls' },
          { text: 'Simulation Configuration Guide', link: '/Simulation_Config_Guide' },
          { text: 'Simulation Logging', link: '/Simulation_Logging' },
          { text: 'Coverage Reports', link: '/coverage/editmode/Report/' },
          { text: 'Development Guide', link: '/Development_Guide' }
        ]
      }
    ],

    socialLinks: [
      { icon: 'github', link: 'https://github.com/PisterLab/micromissiles-unity' }
    ],
    search: {
      provider: 'local'
    }
  }
})
