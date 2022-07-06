
import { Providers, ProviderState } from '@microsoft/mgt-element';
import { Agenda, Login } from '@microsoft/mgt-react';
import React, { useState, useEffect } from 'react';
import logo from './logo.svg';
import './App.css';


function useIsSignedIn(): [boolean] {
  const [isSignedIn, setIsSignedIn] = useState(false);

  useEffect(() => {
    const updateState = () => {
      const provider = Providers.globalProvider;
      setIsSignedIn(provider && provider.state === ProviderState.SignedIn);
    };

    Providers.onProviderUpdated(updateState);
    updateState();

    return () => {
      Providers.removeProviderUpdatedListener(updateState);
    }
  }, []);

  return [isSignedIn];
}

function App() {
  const [isSignedIn] = useIsSignedIn();
  const [apiData, setApiData] = useState('');
  
  
  
  //api://c14905f0-259c-4429-87fe-11b3fcea0168/access_as_user
  useEffect(() => {

    if (isSignedIn){

      

      let provider = Providers.globalProvider;
      console.log('now trying to get backend token');
      provider.getAccessToken({ scopes: ['api://c14905f0-259c-4429-87fe-11b3fcea0168/.default'] }).then((token) => {

          var headers = new Headers();
          var bearer = "Bearer " + token;
          headers.append("Authorization", bearer);
          var options = {
            method: "GET",
            headers: headers
        };

        try {
          fetch('http://localhost:7071/api/SampleFunc', options)
          .then((response) => {
            console.log(response);
              response.json()
            .then(data => setApiData(data))
        });
          
        } catch (error) {
          //quietly log this error. we want app to continue to function
          console.log('fetchFailed', error);
            
        }


      })
      // .catch(err => {
      //   console.log(err);
      // });
    }

  }, [isSignedIn]);




  return (
    <div className="App">

      <header>
      <img src={logo} className="App-logo" alt="logo" />
        <Login />

      </header>
      <div>
        <p>fromAPI</p>
        <p>{apiData}</p>

      </div>
      <div>

        {isSignedIn &&

          <Agenda />}

      </div>
    </div>

  );

}


export default App;