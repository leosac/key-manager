pipeline {
    agent any
    environment {
        DOTNET_SYSTEM_GLOBALIZATION_INVARIANT = 1
    }
    tools {
        dotnetsdk 'dotnet8'
    }
	stages {
        stage('Pre-Build') {
            steps {
                dotnetRestore()
            }
        }
	    stage('Build') {
            steps {
                dotnetBuild()
            }
        }
        stage('Sonar') {
            environment {
                scannerHome = tool 'SonarScanner for MSBuild'
            }
            steps {
                withSonarQubeEnv('SonarQube Community') {
                    sh "dotnet ${scannerHome}/SonarScanner.MSBuild.dll begin /k:'leosac_key-manager_8fc98234-7f0e-4a35-86ea-32e2323a02ca'"
                    dotnetBuild()
                    sh "dotnet ${scannerHome}/SonarScanner.MSBuild.dll end"
                }
                timeout(time: 1, unit: 'HOURS') {
                    waitForQualityGate(abortPipeline: true)
                }
            }
            when {
                anyOf {
                    branch 'main'
                    buildingTag()
                    changeRequest()
                }
            }
        }
        stage('Test') {
            steps {
                dotnetTest()
            }
        }
    }
}