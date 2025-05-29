import requests
import re
import csv
import locale

locale.setlocale(locale.LC_NUMERIC, 'C')

def extract_id(url_or_list):
    if isinstance(url_or_list, list):
        if not url_or_list:
            return None
        url = url_or_list[0]
    else:
        url = url_or_list

    if not url:
        return None

    url = url.strip()
    match = re.search(r'/(\d+)/$', url)
    return int(match.group(1)) if match else None

def get_data(api_url):
    if api_url:
        response = requests.get(api_url)
        data = response.json()
        return data
    else:
        return None
    
def calculate_age(birth_year, reference_year=34):
    if not birth_year or birth_year.lower() == 'unknown':
        return None
    try:
        if birth_year.endswith('BBY'):
            return reference_year + int(birth_year[:-3])
        elif birth_year.endswith('ABY'):
            return reference_year - int(birth_year[:-3])
    except ValueError:
        return None
    return None

def remove_unkown(data):
    for row in data:
        for i in range(len(row)):
            if (row[i] == 'unknown' or row[i] == 'none' or row[i] == 'n/a'):
                row[i] = None

def process_mass(mass):
    if (mass == 'unknown' or mass == 'none' or mass == 'n/a'):
        return mass
    else:
        return mass.replace(",", ".")

    
### PEOPLE

api_url_people = "https://swapi.py4e.com/api/people/"

people_data = []
while api_url_people is not None:
    data = get_data(api_url_people)
    api_url_people = data['next']        
    for person in data['results']:
        people_data.append([
            person['name'],
            calculate_age(person['birth_year']),
            person['eye_color'],
            person['gender'],
            person['height'],
            process_mass(person['mass']),
            extract_id(person['homeworld']),
            extract_id(person['species']),
        ])

remove_unkown(people_data)

with open('people.csv', 'w', newline='', encoding='utf-8') as csvfile:
    csv_writer = csv.writer(csvfile)
    csv_writer.writerow(['name', 'age', 'eye_color', 'gender', 'height', 'mass', 'homeworld_id', 'species_id'])
    csv_writer.writerows(people_data)

print("CSV file created: people.csv")

### PLANETS

api_url_planets = "https://swapi.py4e.com/api/planets"

planets_data = []
id = 1
while api_url_planets is not None:
    data = get_data(api_url_planets)
    api_url_planets = data['next']        
    for planet in data['results']:
        planets_data.append([
            id,
            planet['name'],
            planet['diameter'],
            planet['rotation_period'],
            planet['population'],
            planet['surface_water']
        ])
        id += 1

remove_unkown(planets_data)

with open('planets.csv', 'w', newline='', encoding='utf-8') as csvfile:
    csv_writer = csv.writer(csvfile)
    csv_writer.writerow(['id', 'name', 'diameter', 'rotation_period', 'population', 'surface_water'])
    csv_writer.writerows(planets_data)

print("CSV file created: planets.csv")

### SPECIES

api_url_species = "https://swapi.py4e.com/api/species"

species_data = []
id = 1
while api_url_species is not None:
    data = get_data(api_url_species)
    api_url_species = data['next']        
    for species in data['results']:
        species_data.append([
            id,
            species['name'],
            species['classification'],
            species['designation'],
            species['average_height'],
            species['average_lifespan']
        ])
        id += 1

remove_unkown(species_data)

with open('species.csv', 'w', newline='', encoding='utf-8') as csvfile:
    csv_writer = csv.writer(csvfile)
    csv_writer.writerow(['id', 'name', 'classification', 'designation', 'average_height', 'average_lifespan'])
    csv_writer.writerows(species_data)

print("CSV file created: species.csv")
