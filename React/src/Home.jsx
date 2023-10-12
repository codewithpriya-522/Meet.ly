import React from 'react'
import NavBar from './component/navbar/NavBar'
import Hero from './component/hero/Hero'
import Footer from './component/footer/Footer'
import NewsLetter from './component/newsLetter/NewsLetter'

const Home = () => {
  return (
    <div >
        <NavBar/>
        <Hero/>
        <NewsLetter/>
        <Footer/>
    </div>
  )
}

export default Home